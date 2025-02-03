namespace FF7R2.DataObject.Properties;

public class StrProperty(FrozenObject obj, Property property) : PropertyValue<string>(obj, property) {
    private FStringProxy internalData = new();
    protected override string? Data {
        get => internalData.data;
        set => internalData.data = value;
    }

    public override string? PublicData => Data;

    private long headerPos;

    internal override void Read(BinaryReader reader) {
        reader.BaseStream.Position = reader.BaseStream.Position.Align(8, obj.frozenObjectStart);
        Offset                     = reader.BaseStream.Position;
        internalData               = new();
        internalData.Read(reader);
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        switch (mode) {
            case PropertyWriteMode.MAIN_OBJ_ONLY:
                writer.BaseStream.Position = writer.BaseStream.Position.Align(8, obj.frozenObjectStart);
                headerPos                  = writer.BaseStream.Position;
                writer.WriteHeader(internalData);
                break;
            case PropertyWriteMode.SUB_OBJECTS_ONLY:
                writer.WriteData(internalData, headerPos);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
}