namespace FF7R2.DataObject.Properties;

public class FloatProperty(FrozenObject obj, Property property) : PropertyValue<float>(obj, property) {
    protected override float Data { get; set; }

    public override void Read(BinaryReader reader) {
        reader.BaseStream.Position = reader.BaseStream.Position.Align(4, obj.frozenObjectStart);
        Offset                     = reader.BaseStream.Position;
        Data                       = reader.ReadSingle();
    }

    public override void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }
}