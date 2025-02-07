namespace FF7R2.DataObject.Properties;

public class Int8Property(FrozenObject obj, Property property) : PropertyValue<sbyte>(obj, property) {
    public override sbyte Data { get; set; }

    internal override void Read(BinaryReader reader) {
        Offset = reader.BaseStream.Position;
        Data   = reader.ReadSByte();
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        if (mode == PropertyWriteMode.SUB_OBJECTS_ONLY) return;
        writer.Write(Data);
    }
}