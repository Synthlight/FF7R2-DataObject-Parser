namespace FF7R2.DataObject.Properties;

public class UInt32Property(FrozenObject obj, Property property) : PropertyValue<uint>(obj, property) {
    protected override uint Data { get; set; }

    public override void Read(BinaryReader reader) {
        reader.BaseStream.Position = reader.BaseStream.Position.Align(4, obj.frozenObjectStart);
        Offset                     = reader.BaseStream.Position;
        Data                       = reader.ReadUInt32();
    }

    public override void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }
}