namespace FF7R2.DataObject.Properties;

public class UInt16Property(FrozenObject obj, Property property) : PropertyValue<ushort>(obj, property) {
    protected override ushort Data { get; set; }

    public override void Read(BinaryReader reader) {
        Offset = reader.BaseStream.Position;
        Data   = reader.ReadUInt16();
    }

    public override void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }
}