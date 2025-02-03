namespace FF7R2.DataObject;

public class MemoryImagePtr(InnerAsset asset) {
    public ulong packed;
    public long  OffsetFromThis => (long) packed >> 1;

    internal void Read(BinaryReader reader) {
        packed = reader.ReadUInt64();
    }

    internal void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }
}