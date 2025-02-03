namespace FF7R2.DataObject.Properties;

public class Int32Property(FrozenObject obj, Property property) : PropertyValue<int>(obj, property) {
    protected override int Data { get; set; }

    public override void Read(BinaryReader reader) {
        reader.BaseStream.Position = reader.BaseStream.Position.Align(4, obj.frozenObjectStart);
        Offset                     = reader.BaseStream.Position;
        Data                       = reader.ReadInt32();
    }

    public override void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }
}