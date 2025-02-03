using CUE4Parse.GameTypes.FF7.Assets.Objects.Properties;

namespace FF7R2.DataObject.Properties;

public class Ff7R2ArrayPropertyValue(FrozenObject obj, Property property) : PropertyValue<PropertyValue[]>(obj, property) {
    protected ArrayProxy<PropertyValue> data = new();
    protected override PropertyValue[]? Data {
        get => data.data;
        set {
        }
    }

    public override PropertyValue[]? PublicData => Data;

    private long headerPos;

    internal override void Read(BinaryReader reader) {
        reader.BaseStream.Position = reader.BaseStream.Position.Align(8, obj.frozenObjectStart);
        Offset                     = reader.BaseStream.Position;
        var align = GetEntryAlignment();
        data = new();
        data.Read(reader, () => {
            var propertyValue = property.Create();
            propertyValue.Read(reader);
            return propertyValue;
        }, align, obj.frozenObjectStart);
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        switch (mode) {
            case PropertyWriteMode.MAIN_OBJ_ONLY:
                writer.BaseStream.Position = writer.BaseStream.Position.Align(8, obj.frozenObjectStart);
                headerPos                  = writer.BaseStream.Position;
                writer.WriteHeader(data);
                break;
            case PropertyWriteMode.SUB_OBJECTS_ONLY:
                var align = GetEntryAlignment();
                writer.WriteData(data, headerPos, value => { value.Write(writer, mode); }, align, obj.frozenObjectStart);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    private int GetEntryAlignment() {
        return property.UnderlyingType switch {
            FF7propertyType.BoolProperty => 1,
            FF7propertyType.ByteProperty => 1,
            FF7propertyType.Int8Property => 1,
            FF7propertyType.UInt16Property => 2,
            FF7propertyType.Int16Property => 2,
            _ => 4
        };
    }
}