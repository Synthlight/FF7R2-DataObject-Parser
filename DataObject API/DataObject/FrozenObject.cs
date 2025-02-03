using System.Diagnostics;
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
    private Key[]                   keys;
    private Property[]              properties;
    private Entry[]                 entries;

    public Dictionary<long, FName> OffsetToNameLookup = [];
    public Dictionary<Key, Entry>  DataTable          = [];

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

        reader.BaseStream.Seek(frozenObjectStart, SeekOrigin.Begin);
        keys = reader.ReadArrayProxy(asset, (_, _) => {
            var key = new Key(this);
            key.Read(reader);
            return key;
        });

        reader.BaseStream.Seek(40, SeekOrigin.Current);
        properties = reader.ReadArrayProxy(asset, (_, _) => {
            var property = new Property(this);
            property.Read(reader);
            return property;
        });

        entries = reader.ReadArrayProxy(asset, (_, _) => {
            var value = new Entry(this, properties);
            value.Read(reader);
            return value;
        });

        DataTable = [];
        for (var i = 0; i < keys.Length; i++) {
            DataTable[keys[i]] = entries[i];
        }
    }

    public void Write(BinaryWriter writer) {
        var frozenSizeAddress = writer.BaseStream.Position;

        // Ends where the vTable count begins.
        var frozenSize = 100; // TODO: Calculate this.
        writer.Write(0);

        writer.Write(size);
        writer.Write(unk1);
        writer.Write(padding);
        writer.BaseStream.Position += padding;
        frozenObjectStart          =  writer.BaseStream.Position;

        writer.BaseStream.Seek(frozenSize, SeekOrigin.Current);
        writer.Write(vTables.Count);
        writer.Write(scriptNames.Count);
        writer.Write(minimalNames.Count);

        // TODO
        throw new NotImplementedException();
    }
}

public static class FrozenObjectExtensions {
    public static void Write(this BinaryWriter writer, FrozenObject obj) {
        obj.Write(writer);
    }
}