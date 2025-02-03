namespace FF7R2.DataObject.Properties;

public class Int64Property(FrozenObject obj, Property property) : PropertyValue<long>(obj, property) {
    public override long Data { get; set; }

    internal override void Read(BinaryReader reader) {
        Offset = reader.BaseStream.Position;
        Data   = reader.ReadInt64();
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        writer.Write(Data);
    }
}