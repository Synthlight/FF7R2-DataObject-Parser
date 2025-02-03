namespace FF7R2.DataObject.Properties;

public class Int8Property(FrozenObject obj, Property property) : PropertyValue<sbyte>(obj, property) {
    protected override sbyte Data { get; set; }

    internal override void Read(BinaryReader reader) {
        Offset = reader.BaseStream.Position;
        Data   = reader.ReadSByte();
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        writer.Write(Data);
    }
}