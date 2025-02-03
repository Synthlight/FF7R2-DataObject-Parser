using CUE4Parse.GameTypes.FF7.Assets.Objects.Properties;
using CUE4Parse.UE4.Exceptions;
using CUE4Parse.UE4.Objects.UObject;

namespace FF7R2.DataObject.Properties;

public class Property(FrozenObject obj) {
    public FName           Name;
    public FF7propertyType UnderlyingType;

    public void Read(BinaryReader reader) {
        Name = obj.OffsetToNameLookup[reader.BaseStream.Position - obj.frozenObjectStart];
        reader.BaseStream.Seek(8, SeekOrigin.Current); // Skip the placeholder FName bytes.
        UnderlyingType = (FF7propertyType) reader.ReadInt32();
    }

    public void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }

    public override string ToString() {
        return $"{Name} :: {UnderlyingType}";
    }

    public PropertyValue Create(FrozenObject obj) {
        return UnderlyingType switch {
            FF7propertyType.BoolProperty => new BoolProperty(obj, this),
            FF7propertyType.ByteProperty => new ByteProperty(obj, this),
            FF7propertyType.Int8Property => new Int8Property(obj, this),
            FF7propertyType.UInt16Property => new UInt16Property(obj, this),
            FF7propertyType.Int16Property => new Int16Property(obj, this),
            FF7propertyType.UIntProperty => new UInt32Property(obj, this),
            FF7propertyType.IntProperty => new Int32Property(obj, this),
            FF7propertyType.Int64Property => new Int64Property(obj, this),
            FF7propertyType.FloatProperty => new FloatProperty(obj, this),
            FF7propertyType.StrProperty => new StrProperty(obj, this),
            FF7propertyType.NameProperty => new NameProperty(obj, this),
            _ => throw new ParserException($"Unknown property type {UnderlyingType}")
        };
    }
}