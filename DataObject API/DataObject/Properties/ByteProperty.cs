namespace FF7R2.DataObject.Properties;

public class ByteProperty(FrozenObject obj, Property property) : PropertyValue<byte>(obj, property) {
    public override byte Data { get; set; }

    internal override void Read(BinaryReader reader) {
        Offset = reader.BaseStream.Position;
        Data   = reader.ReadByte();
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        if (mode == PropertyWriteMode.SUB_OBJECTS_ONLY) return;
        writer.Write(Data);
    }
}