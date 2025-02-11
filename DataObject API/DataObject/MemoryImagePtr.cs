using Org.BouncyCastle.Bcpg;

namespace FF7R2.DataObject;

public class MemoryImagePtr {
    public ulong packed;

    internal bool hasData {
        get => (packed & 1) == 1;
        set {
            if (value) {
                packed |= 1UL;
            } else {
                packed &= ~1UL;
            }
        }
    }

    public long OffsetFromThis {
        get => (long) packed >> 1;
        set => packed = (hasData ? 1UL : 0UL) | (ulong) (value << 1);
    }

    internal void Read(BinaryReader reader) {
        packed = reader.ReadUInt64();
    }

    internal void Write(BinaryWriter writer) {
        writer.Write(packed);
    }
}

public static class MemoryImagePtrExtensions {
    internal static void Write(this BinaryWriter writer, MemoryImagePtr obj) {
        obj.Write(writer);
    }
}