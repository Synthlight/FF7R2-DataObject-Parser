namespace FF7R2.DataObject.Properties;

public class Ff7R2UInt64Property(FrozenObject obj, Property property) : PropertyValue<ulong>(obj, property) {
    protected override ulong Data { get; set; }

    public override void Read(BinaryReader reader) {
        Offset = reader.BaseStream.Position;
        Data   = reader.ReadUInt64();
    }

    public override void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }
}