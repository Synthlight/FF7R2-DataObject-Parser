namespace FF7R2.DataObject.Properties;

public class Int8Property(FrozenObject obj, Property property) : PropertyValue<sbyte>(obj, property) {
    protected override sbyte Data { get; set; }

    public override void Read(BinaryReader reader) {
        Offset = reader.BaseStream.Position;
        Data   = reader.ReadSByte();
    }

    public override void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }
}