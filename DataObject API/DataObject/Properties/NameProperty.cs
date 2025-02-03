using CUE4Parse.UE4.Objects.UObject;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FF7R2.DataObject.Properties;

public class NameProperty(FrozenObject obj, Property property) : PropertyValue<FName?>(obj, property) {
    protected override FName? Data { get; set; }

    public override FName? PublicData => Data;

    internal override void Read(BinaryReader reader) {
        reader.BaseStream.Position = reader.BaseStream.Position.Align(4, obj.frozenObjectStart);
        Offset                     = reader.BaseStream.Position;
        if (obj.OffsetToNameLookup.ContainsKey(reader.BaseStream.Position - obj.frozenObjectStart)) Data = obj.OffsetToNameLookup[reader.BaseStream.Position - obj.frozenObjectStart];
        reader.BaseStream.Seek(8, SeekOrigin.Current); // Skip the placeholder FName bytes.
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        writer.BaseStream.Position = writer.BaseStream.Position.Align(4, obj.frozenObjectStart);
        writer.BaseStream.Seek(8, SeekOrigin.Current); // Skip the placeholder FName bytes.
        // TODO: Do something to update the offset map entries of the obj here.◘
    }
}