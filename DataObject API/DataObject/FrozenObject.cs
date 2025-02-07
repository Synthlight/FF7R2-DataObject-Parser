using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CUE4Parse.UE4.Objects.UObject;
using FF7R2.DataObject.Properties;

namespace FF7R2.DataObject;

public class FrozenObject(InnerAsset asset) {
    public readonly InnerAsset asset = asset;
    internal        long       frozenObjectStart; // What's added to pos when aligning.

    private uint                    size;
    private ushort                  unk1;
    private ushort                  padding;
    private List<MemoryImageVTable> vTables      = [];
    private List<MemoryImageName>   scriptNames  = [];
    private List<MemoryImageName>   minimalNames = [];
    private SparseArrayProxy<Key>   keys         = new();
    private ArrayProxy<int>         indexes      = new();
    public  ArrayProxy<Property>    properties   = new();
    private ArrayProxy<Entry>       entries      = new();

    public Dictionary<long, FName> OffsetToNameLookup = [];
    [SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
    public Dictionary<FName, Entry> DataTable = [];

    internal void Read(BinaryReader reader) {
        var frozenSize = reader.ReadUInt32();
        size                       =  reader.ReadUInt32();
        unk1                       =  reader.ReadUInt16();
        padding                    =  reader.ReadUInt16();
        reader.BaseStream.Position += padding;

        // This is where the frozen archive 'begins' and what it considers pos 0 internally when reading.
        frozenObjectStart = reader.BaseStream.Position;

        Debug.WriteLine($"FrozenArchive pos: {frozenObjectStart}");
        reader.BaseStream.Seek(frozenSize, SeekOrigin.Current);

        var numVTables      = reader.ReadInt32();
        var numScriptNames  = reader.ReadInt32();
        var numMinimalNames = reader.ReadInt32();

        vTables = [];
        for (var i = 0; i < numVTables; i++) {
            var vTable = new MemoryImageVTable(asset);
            vTable.Read(reader);
            vTables.Add(vTable);
        }

        scriptNames = [];
        for (var i = 0; i < numScriptNames; i++) {
            var name = new MemoryImageName(asset);
            name.Read(reader);
            scriptNames.Add(name);
        }

        minimalNames       = [];
        OffsetToNameLookup = [];
        for (var i = 0; i < numMinimalNames; i++) {
            var imageName = new MemoryImageName(asset);
            imageName.Read(reader);
            minimalNames.Add(imageName);
            foreach (var offset in imageName.offsets) {
                OffsetToNameLookup[offset] = imageName.name;
            }
        }

        // Might need to read as a linked list?
        reader.BaseStream.Seek(frozenObjectStart, SeekOrigin.Begin);
        keys = new();
        keys.Read(reader, () => {
            var key = new Key(this);
            key.Read(reader);
            return key;
        });

        indexes = new();
        indexes.Read(reader, reader.ReadInt32);

        properties = new();
        properties.Read(reader, () => {
            var property = new Property(this);
            property.Read(reader);
            return property;
        });

        entries = new();
        entries.Read(reader, () => {
            var value = new Entry(this, properties.data);
            value.Read(reader);
            return value;
        });

        DataTable = [];
        for (var i = 0; i < keys.data.Length; i++) {
            DataTable[keys.data[i].name] = entries.data[i];
        }
    }

    internal void Write(BinaryWriter writer) {
        // Ends where the vTable count begins.
        var frozenSizePos = writer.BaseStream.Position;
        var frozenSize    = 0;
        writer.Write(frozenSize); // Update later.

        writer.Write(size);
        writer.Write(unk1);
        writer.Write(padding);
        writer.BaseStream.Position += padding;
        frozenObjectStart          =  writer.BaseStream.Position;

        writer.WriteHeader(keys);
        writer.WriteHeader(indexes);
        writer.WriteHeader(properties);
        writer.WriteHeader(entries);

        // TODO: How names are done break my brain. Just write them as-is and make it readonly for now.
        writer.WriteData(keys, writer.Write, frozenObjectStart);
        writer.WriteData(indexes, writer.Write);
        writer.WriteData(properties, writer.Write);

        // Write all the base entries, *then* go back and write all the array data.
        writer.WriteData(entries, entry => writer.Write(entry, PropertyWriteMode.MAIN_OBJ_ONLY));
        foreach (var entry in entries.data) {
            writer.Write(entry, PropertyWriteMode.SUB_OBJECTS_ONLY);
        }
        //writer.WriteData(entries, entry => writer.Write(entry, PropertyWriteMode.SUB_OBJECTS_ONLY));

        writer.BaseStream.Position = writer.BaseStream.Position.Align(8);

        var frozenEnd = writer.BaseStream.Position;
        frozenSize                 = (int) (frozenEnd - frozenObjectStart);
        writer.BaseStream.Position = frozenSizePos;
        writer.Write(frozenSize);

        writer.BaseStream.Seek(frozenEnd, SeekOrigin.Begin);
        writer.Write(vTables.Count);
        writer.Write(scriptNames.Count);
        writer.Write(minimalNames.Count);

        foreach (var vTable in vTables) {
            writer.Write(vTable);
        }
        foreach (var scriptName in scriptNames) {
            writer.Write(scriptName);
        }

        // Re-create the whole minimal name map thing.
        // Our expansion of the data *should* be creating more name offsets, so we need to be updating the list here with them.
        minimalNames.Clear();
        foreach (var key in keys.data) {
            var offset = (uint) (key.offset - frozenObjectStart);
            AddOffsetToMinimalNames(key.name, offset);
        }
        foreach (var property in properties.data) {
            var offset = (uint) (property.offset - frozenObjectStart);
            AddOffsetToMinimalNames(property.name, offset);
        }
        foreach (var entry in entries.data) {
            UpdateMinimalNamesRecursively(entry.propertyValues);
        }

        foreach (var minimalName in from name in minimalNames
                                    orderby name.name
                                    select name) {
            //Debug.Assert(minimalName.offsets.Count > 0);
            writer.Write(minimalName);
        }
    }

    public void UpdateMinimalNamesRecursively(IEnumerable<PropertyValue> data) {
        foreach (var value in data) {
            switch (value) {
                case NameProperty nameProp: {
                    if (nameProp.Data == null) continue;
                    var offset = (uint) (nameProp.Offset - frozenObjectStart);
                    AddOffsetToMinimalNames((FName) nameProp.Data, offset);
                    break;
                }
                case ArrayProperty arrayProp:
                    UpdateMinimalNamesRecursively(arrayProp.Data!);
                    break;
            }
        }
    }

    private void AddOffsetToMinimalNames(FName name, uint offset) {
        var minimalName = minimalNames.FirstOrDefault(imageName => imageName.name == name);
        if (minimalName == null) {
            minimalName = new(asset) {name = name};
            minimalNames.Add(minimalName);
        }

        if (!minimalName.offsets.Contains(offset)) minimalName.offsets.Add(offset);
    }
}

public static class FrozenObjectExtensions {
    internal static void Write(this BinaryWriter writer, FrozenObject obj) {
        obj.Write(writer);
    }
}