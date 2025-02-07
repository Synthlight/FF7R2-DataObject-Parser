namespace FF7R2.DataObject.Properties;

public class UInt32Property(FrozenObject obj, Property property) : PropertyValue<uint>(obj, property) {
    public override uint Data { get; set; }

    internal override void Read(BinaryReader reader) {
        reader.BaseStream.Position = reader.BaseStream.Position.Align(4, obj.frozenObjectStart);
        Offset                     = reader.BaseStream.Position;
        Data                       = reader.ReadUInt32();
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        if (mode == PropertyWriteMode.SUB_OBJECTS_ONLY) return;
        writer.BaseStream.Position = writer.BaseStream.Position.Align(4, obj.frozenObjectStart);
        writer.Write(Data);
    }
}