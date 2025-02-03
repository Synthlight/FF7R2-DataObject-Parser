using CUE4Parse.UE4.Objects.UObject;

namespace FF7R2.DataObject;

public class Key(FrozenObject obj) {
    public FName Name;
    public int   Index;
    public int   type;
    public uint  unk1;

    public void Read(BinaryReader reader) {
        Name = obj.OffsetToNameLookup[reader.BaseStream.Position - obj.frozenObjectStart];
        reader.BaseStream.Seek(8, SeekOrigin.Current); // Skip the placeholder FName bytes.
        Index = reader.ReadInt32();
        type  = reader.ReadInt32();
        unk1  = reader.ReadUInt32();
    }

    public override string ToString() {
        return $"{Name}";
    }
}