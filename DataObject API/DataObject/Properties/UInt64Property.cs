namespace FF7R2.DataObject.Properties;

public class UInt64Property(FrozenObject obj, Property property) : PropertyValue<ulong>(obj, property) {
    public override ulong Data { get; set; }

    internal override void Read(BinaryReader reader) {
        Offset = reader.BaseStream.Position;
        Data   = reader.ReadUInt64();
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        if (mode == PropertyWriteMode.SUB_OBJECTS_ONLY) return;
        writer.Write(Data);
    }
}