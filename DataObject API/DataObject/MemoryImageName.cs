using CUE4Parse.UE4.Objects.UObject;

namespace FF7R2.DataObject;

public class MemoryImageName(InnerAsset asset) {
    public FName      name;
    public List<uint> offsets = [];

    internal void Read(BinaryReader reader) {
        name = reader.ReadFName(asset.ioStoreAsset.names);
        var count = reader.ReadUInt32();
        offsets = [];
        for (var i = 0; i < count; i++) {
            var offset = reader.ReadUInt32();
            offsets.Add(offset);
        }
    }

    internal void Write(BinaryWriter writer) {
        writer.Write(name);
        writer.Write(offsets.Count);
        foreach (var offset in offsets) {
            writer.Write(offset);
        }
    }

    public override string ToString() {
        return name.ToString();
    }
}

public static class MemoryImageNameExtensions {
    internal static void Write(this BinaryWriter writer, MemoryImageName obj) {
        obj.Write(writer);
    }
}