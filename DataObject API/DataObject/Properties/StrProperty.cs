namespace FF7R2.DataObject.Properties;

public class StrProperty(FrozenObject obj, Property property) : PropertyValue<string>(obj, property) {
    private FStringProxy internalData = new(obj);
    public override string? Data {
        get => internalData.data;
        set => internalData.data = value;
    }

    public override string? DataAsByteProxy {
        get => Data;
        set => Data = value;
    }

    internal override void Read(BinaryReader reader) {
        reader.BaseStream.Position = reader.BaseStream.Position.Align(8, obj.frozenObjectStart);
        Offset                     = reader.BaseStream.Position;
        internalData               = new(obj);
        internalData.Read(reader);
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        switch (mode) {
            case PropertyWriteMode.MAIN_OBJ_ONLY:
                writer.BaseStream.Position = writer.BaseStream.Position.Align(8, obj.frozenObjectStart);
                writer.WriteHeader(internalData);
                break;
            case PropertyWriteMode.SUB_OBJECTS_ONLY:
                writer.BaseStream.Position = writer.BaseStream.Position.Align(2, obj.frozenObjectStart);
                writer.WriteData(internalData);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
}