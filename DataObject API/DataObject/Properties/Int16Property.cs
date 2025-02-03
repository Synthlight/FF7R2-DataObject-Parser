namespace FF7R2.DataObject.Properties;

public class Int16Property(FrozenObject obj, Property property) : PropertyValue<short>(obj, property) {
    protected override short Data { get; set; }

    internal override void Read(BinaryReader reader) {
        Offset = reader.BaseStream.Position;
        Data   = reader.ReadInt16();
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        writer.Write(Data);
    }
}