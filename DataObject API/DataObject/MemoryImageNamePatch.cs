namespace FF7R2.DataObject;

public class MemoryImageNamePatch(InnerAsset asset) {
    public uint Offset;

    internal void Read(BinaryReader reader) {
        Offset = reader.ReadUInt32();
    }

    internal void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }
}