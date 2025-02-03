namespace FF7R2.DataObject.Properties;

public class BoolProperty(FrozenObject obj, Property property) : PropertyValue<bool>(obj, property) {
    protected override bool Data { get; set; }

    public override void Read(BinaryReader reader) {
        Offset = reader.BaseStream.Position;
        Data   = reader.ReadBoolean();
    }

    public override void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }
}