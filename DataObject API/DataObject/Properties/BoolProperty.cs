namespace FF7R2.DataObject.Properties;

public class BoolProperty(FrozenObject obj, Property property) : PropertyValue<bool>(obj, property) {
    public override bool Data { get; set; }

    internal override void Read(BinaryReader reader) {
        Offset = reader.BaseStream.Position;
        Data   = reader.ReadBoolean();
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        writer.Write(Data);
    }
}