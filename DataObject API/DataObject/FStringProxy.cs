using System.Text;

namespace FF7R2.DataObject;

public class FStringProxy(FrozenObject obj) : ICachableObject {
    private  MemoryImagePtr dataPtr = new();
    internal string?        data;
    private  int            charMax;

    private long headerPos;
    private bool skipDataWrite;

    internal void Read(BinaryReader reader) {
        var initialPos = reader.BaseStream.Position;
        dataPtr = new();
        dataPtr.Read(reader);
        var charNum = reader.ReadInt32();
        charMax = reader.ReadInt32();

        if (charNum < 1) return;

        var continuePos = reader.BaseStream.Position;
        reader.BaseStream.Position = initialPos + dataPtr.OffsetFromThis;

        var bytes = reader.ReadBytes(charNum * 2);
        data = Encoding.Unicode.GetString(bytes, 0, bytes.Length - 2);

        reader.BaseStream.Position = continuePos;
    }

    internal void WriteHeader(BinaryWriter writer) {
        dataPtr.hasData = data is {Length: > 0};
        headerPos       = writer.BaseStream.Position;
        writer.Write(dataPtr); // Update later.
        writer.Write(data?.Length + 1 ?? 0); // +1 for null term.
        writer.Write(charMax);
    }

    internal void WriteData(BinaryWriter writer) {
        var initialPos = writer.BaseStream.Position;

        if (data == null || data?.Length == 0) {
            if (headerPos == 0) throw new("Header position not set.");
            writer.BaseStream.Position = headerPos;
            dataPtr.OffsetFromThis     = 0;
            writer.Write(dataPtr);
            writer.BaseStream.Position = initialPos;
            return;
        }

        if (headerPos == 0) throw new("Header position not set.");
        writer.BaseStream.Position = headerPos;
        dataPtr.OffsetFromThis     = initialPos - headerPos;

        var self = new CachedObject(dataPtr.OffsetFromThis, this);
        var hash = self.GetHashCode();
        skipDataWrite = false;
        if (obj.objectCache.TryGetValue(hash, out var cachedObj)) {
            //dataPtr.OffsetFromThis = cachedObj.offset;
            //skipDataWrite          = true;
        } else {
            //obj.objectCache[hash] = self;
        }

        writer.Write(dataPtr);

        if (skipDataWrite) return;
        writer.BaseStream.Position = headerPos + dataPtr.OffsetFromThis;

        var bytes = Encoding.Unicode.GetBytes(data!);
        Array.Resize(ref bytes, bytes.Length + 2);
        writer.Write(bytes);
    }

    public int GetCacheHash() {
        return data?.GetHashCode() ?? 0;
    }
}

public static class FStringProxyExtensions {
    internal static void WriteHeader(this BinaryWriter writer, FStringProxy obj) {
        obj.WriteHeader(writer);
    }

    internal static void WriteData(this BinaryWriter writer, FStringProxy obj) {
        obj.WriteData(writer);
    }
}