using CUE4Parse.GameTypes.FF7.Assets.Objects.Properties;

namespace FF7R2.DataObject.Properties;

public class Ff7R2ArrayPropertyValue(FrozenObject obj, Property property) : PropertyValue<PropertyValue[]>(obj, property) {
    protected override PropertyValue[]? Data { get; set; }
    public             long[]?          Offsets;

    public override PropertyValue[]? PublicData => Data;

    public override void Read(BinaryReader reader) {
        reader.BaseStream.Position = reader.BaseStream.Position.Align(8, obj.frozenObjectStart);
        Offset                     = reader.BaseStream.Position;
        var align = property.UnderlyingType switch {
            FF7propertyType.BoolProperty => 1,
            FF7propertyType.ByteProperty => 1,
            FF7propertyType.Int8Property => 1,
            FF7propertyType.UInt16Property => 2,
            FF7propertyType.Int16Property => 2,
            _ => 4
        };
        Data = reader.ReadArrayProxy(obj.asset, (count, index) => {
            Offsets ??= new long[count];
            var propertyValue = property.Create(obj);
            Offsets[index] = reader.BaseStream.Position;
            propertyValue.Read(reader);
            return propertyValue;
        }, align);
    }

    public override void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }
}