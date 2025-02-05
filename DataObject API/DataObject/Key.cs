using CUE4Parse.UE4.Objects.UObject;

namespace FF7R2.DataObject;

public class Key(FrozenObject obj) {
    public FName name;
    public int   index;
    public int   nextIndex;
    public uint  priority;

    internal long offset; // For updating minimal names.

    internal void Read(BinaryReader reader) {
        offset = reader.BaseStream.Position;
        name   = obj.OffsetToNameLookup[offset - obj.frozenObjectStart];
        reader.BaseStream.Seek(8, SeekOrigin.Current); // Skip the placeholder FName bytes.
        index     = reader.ReadInt32();
        nextIndex = reader.ReadInt32();
        priority  = reader.ReadUInt32();
    }

    internal void Write(BinaryWriter writer) {
        offset = writer.BaseStream.Position;
        writer.BaseStream.Seek(8, SeekOrigin.Current); // Skip the placeholder FName bytes.
        writer.Write(index);
        writer.Write(nextIndex);
        writer.Write(priority);
    }

    public override string ToString() {
        return $"{name}";
    }
}

public static class KeyExtensions {
    internal static void Write(this BinaryWriter writer, Key obj) {
        obj.Write(writer);
    }
}