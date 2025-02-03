namespace FF7R2.DataObject.Properties;

public class StrProperty(FrozenObject obj, Property property) : PropertyValue<string>(obj, property) {
    protected override string? Data { get; set; }

    public override string? PublicData => Data;

    public override void Read(BinaryReader reader) {
        reader.BaseStream.Position = reader.BaseStream.Position.Align(8, obj.frozenObjectStart);
        Offset                     = reader.BaseStream.Position;
        Data                       = reader.ReadFStringProxy(obj.asset);
    }

    public override void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }
}