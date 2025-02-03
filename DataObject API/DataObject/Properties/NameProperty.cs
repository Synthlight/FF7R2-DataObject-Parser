using CUE4Parse.UE4.Objects.UObject;

namespace FF7R2.DataObject.Properties;

public class NameProperty(FrozenObject obj, Property property) : PropertyValue<FName?>(obj, property) {
    protected override FName? Data { get; set; }

    public override FName? PublicData => Data;

    public override void Read(BinaryReader reader) {
        reader.BaseStream.Position = reader.BaseStream.Position.Align(4, obj.frozenObjectStart);
        Offset                     = reader.BaseStream.Position;
        if (obj.OffsetToNameLookup.ContainsKey(reader.BaseStream.Position - obj.frozenObjectStart)) Data = obj.OffsetToNameLookup[reader.BaseStream.Position - obj.frozenObjectStart];
        reader.BaseStream.Seek(8, SeekOrigin.Current); // Skip the placeholder FName bytes.
    }

    public override void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }
}