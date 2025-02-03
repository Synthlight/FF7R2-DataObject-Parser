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
    private ArrayProxy<Property>    properties   = new();
    private ArrayProxy<Entry>       entries      = new();

    public Dictionary<long, FName> OffsetToNameLookup = [];
    [SuppressMessage("ReSharper", "CollectionNeverQueried.Global")]
    public Dictionary<Key, Entry> DataTable = [];

    public void Read(BinaryReader reader) {
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
            foreach (var patch in imageName.patches) {
                OffsetToNameLookup[patch.Offset] = imageName.name;
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
            DataTable[keys.data[i]] = entries.data[i];
        }
    }

    public void Write(BinaryWriter writer) {
        // Ends where the vTable count begins.
        var frozenSizePos = writer.BaseStream.Position;
        var frozenSize    = 0;
        writer.Write(frozenSize); // Update later.

        writer.Write(size);
        writer.Write(unk1);
        writer.Write(padding);
        writer.BaseStream.Position += padding;
        frozenObjectStart          =  writer.BaseStream.Position;

        var keyHeaderPos = writer.BaseStream.Position;
        writer.WriteHeader(keys);
        var indexesHeaderPos = writer.BaseStream.Position;
        writer.WriteHeader(indexes);
        var propertiesHeaderPos = writer.BaseStream.Position;
        writer.WriteHeader(properties);
        var entriesHeaderPos = writer.BaseStream.Position;
        writer.WriteHeader(entries);

        // TODO: How names are done break my brain. Just write them as-is and make it readonly for now.
        writer.BaseStream.Seek(frozenSize, SeekOrigin.Current);
        writer.WriteData(keys, keyHeaderPos, writer.Write);

        indexes.WriteData(writer, indexesHeaderPos, writer.Write);
        properties.WriteData(writer, propertiesHeaderPos, writer.Write);

        writer.BaseStream.Seek(frozenSize, SeekOrigin.Current);
        //writer.Write(vTables.Count);
        //writer.Write(scriptNames.Count);
        //writer.Write(minimalNames.Count);

        // TODO
        throw new NotImplementedException();
    }
}

public static class FrozenObjectExtensions {
    public static void Write(this BinaryWriter writer, FrozenObject obj) {
        obj.Write(writer);
    }
}