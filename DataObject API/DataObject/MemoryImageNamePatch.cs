namespace FF7R2.DataObject;

public class MemoryImageNamePatch {
    public uint offset;

    internal void Read(BinaryReader reader) {
        offset = reader.ReadUInt32();
    }

    internal void Write(BinaryWriter writer) {
        writer.Write(offset);
    }
}

public static class MemoryImageNamePatchExtensions {
    internal static void Write(this BinaryWriter writer, MemoryImageNamePatch obj) {
        obj.Write(writer);
    }
}