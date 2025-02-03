namespace FF7R2.DataObject.Properties;

public class Int64Property(FrozenObject obj, Property property) : PropertyValue<long>(obj, property) {
    protected override long Data { get; set; }

    public override void Read(BinaryReader reader) {
        Offset = reader.BaseStream.Position;
        Data   = reader.ReadInt64();
    }

    public override void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }
}