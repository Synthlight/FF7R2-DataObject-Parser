using CUE4Parse.UE4.Objects.UObject;

namespace FF7R2.DataObject.Properties;

public class NameProperty(FrozenObject obj, Property property) : PropertyValue<FName?>(obj, property) {
    public override FName? Data { get; set; }

    public override FName? DataAsByteProxy {
        get => Data;
        set => throw new("Not possible to set this data in byte proxy mode.");
    }

    internal override void Read(BinaryReader reader) {
        reader.BaseStream.Position = reader.BaseStream.Position.Align(4, obj.frozenObjectStart);
        Offset                     = reader.BaseStream.Position;
        if (obj.OffsetToNameLookup.ContainsKey((uint) (reader.BaseStream.Position - obj.frozenObjectStart))) Data = obj.OffsetToNameLookup[(uint) (reader.BaseStream.Position - obj.frozenObjectStart)];
        reader.BaseStream.Seek(8, SeekOrigin.Current); // Skip the placeholder FName bytes.
    }

    internal override void Write(BinaryWriter writer, PropertyWriteMode mode) {
        if (mode == PropertyWriteMode.SUB_OBJECTS_ONLY) return;
        writer.BaseStream.Position = writer.BaseStream.Position.Align(4, obj.frozenObjectStart);
        Offset                     = writer.BaseStream.Position;
        writer.BaseStream.Seek(8, SeekOrigin.Current); // Skip the placeholder FName bytes.
    }
}