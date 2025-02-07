namespace FF7R2.DataObject.Properties;

public class FloatProperty(FrozenObject obj, Property property) : PropertyValue<float>(obj, property) {
    public override float Data { get; set; }

    internal override void Read(BinaryReader reader) {
        reader.BaseStream.Position = reader.BaseStream.Position.Align(4, obj.frozenObjectStart);
        Offset                     = reader.BaseStream.Position;
        Data                       = reader.ReadSingle();
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        if (mode == PropertyWriteMode.SUB_OBJECTS_ONLY) return;
        writer.BaseStream.Position = writer.BaseStream.Position.Align(4, obj.frozenObjectStart);
        writer.Write(Data);
    }
}