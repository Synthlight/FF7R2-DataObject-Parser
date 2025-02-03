namespace FF7R2.DataObject.Properties;

public class UInt16Property(FrozenObject obj, Property property) : PropertyValue<ushort>(obj, property) {
    public override ushort Data { get; set; }

    internal override void Read(BinaryReader reader) {
        Offset = reader.BaseStream.Position;
        Data   = reader.ReadUInt16();
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        writer.Write(Data);
    }
}