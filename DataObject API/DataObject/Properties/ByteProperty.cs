namespace FF7R2.DataObject.Properties;

public class ByteProperty(FrozenObject obj, Property property) : PropertyValue<byte>(obj, property) {
    protected override byte Data { get; set; }

    public override void Read(BinaryReader reader) {
        Offset = reader.BaseStream.Position;
        Data   = reader.ReadByte();
    }

    public override void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }
}