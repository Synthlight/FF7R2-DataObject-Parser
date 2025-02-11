namespace FF7R2.DataObject;

public class ArrayProxy<T>(FrozenObject obj) : ICachableObject {
    internal MemoryImagePtr dataPtr = new();
    public   List<T>        data    = [];

    private long headerPos;
    private bool skipDataWrite;

    internal void Read(BinaryReader reader, Func<T> readEntry, int? align = null, long alignOffset = 0) {
        var initialPos = reader.BaseStream.Position;
        dataPtr = new();
        dataPtr.Read(reader);
        var dataNum = reader.ReadInt32();
        reader.ReadInt32(); // Skip dataMax.

        if (dataNum == 0) return;

        var continuePos = reader.BaseStream.Position;
        reader.BaseStream.Position = initialPos + dataPtr.OffsetFromThis;

        data = new(dataNum);
        for (var i = 0; i < dataNum; ++i) {
            data.Add(readEntry());
            if (align != null) {
                reader.BaseStream.Position = reader.BaseStream.Position.Align((int) align, alignOffset);
            }
        }

        reader.BaseStream.Position = continuePos;
    }

    internal void WriteHeader(BinaryWriter writer) {
        dataPtr.hasData = data.Count > 0;
        headerPos       = writer.BaseStream.Position;
        writer.Write(dataPtr); // Update later.
        writer.Write(data.Count);
        writer.Write(data.Count);
    }

    internal void WriteDataHeaders(BinaryWriter writer, Action<T> writeEntry, int? align = null, long alignOffset = 0) {
        // Align first so the OffsetFromThis points to the exact start and doesn't rely on reading to align first.
        if (align != null) {
            writer.BaseStream.Position = writer.BaseStream.Position.Align((int) align, alignOffset);
        }

        var initialPos = writer.BaseStream.Position;

        if (data.Count == 0) {
            if (headerPos == 0) throw new("Header position not set.");
            writer.BaseStream.Position = headerPos;
            dataPtr.OffsetFromThis     = 0;
            writer.Write(dataPtr);
            writer.BaseStream.Position = initialPos;
            return;
        }

        if (headerPos == 0) throw new("Header position not set.");
        writer.BaseStream.Position = headerPos;
        dataPtr.OffsetFromThis     = initialPos - headerPos; // This is being updated when we write the inner contents and that's bad.

        var self = new CachedObject(initialPos, this);
        var hash = self.GetHashCode();
        skipDataWrite = false;

        if (hash != 0) {
            if (obj.objectCache.TryGetValue(hash, out var cachedObj)) {
                dataPtr.OffsetFromThis = cachedObj.absoluteDataOffset - headerPos;
                skipDataWrite          = true;
            } else {
                obj.objectCache[hash] = self;
            }
        }

        writer.Write(dataPtr);

        if (skipDataWrite) {
            writer.BaseStream.Position = initialPos;
            return;
        }
        writer.BaseStream.Position = headerPos + dataPtr.OffsetFromThis;

        foreach (var entry in data) {
            writeEntry(entry);
        }
    }

    internal void WriteData(BinaryWriter writer, Action<T> writeEntry, int? align = null, long alignOffset = 0) {
        if (skipDataWrite) return;
        foreach (var entry in data) {
            writeEntry(entry);
            if (align != null) {
                writer.BaseStream.Position = writer.BaseStream.Position.Align((int) align, alignOffset);
            }
        }
    }

    public override int GetHashCode() {
        return GetCacheHash();
    }

    public int GetCacheHash() {
        return data.GetListHashCode();
    }
}

public static class ArrayProxyExtensions {
    internal static void WriteHeader<T>(this BinaryWriter writer, ArrayProxy<T> obj) {
        obj.WriteHeader(writer);
    }

    internal static void WriteData<T>(this BinaryWriter writer, ArrayProxy<T> obj, Action<T> writeEntry, int? align = null, long alignOffset = 0) {
        obj.WriteData(writer, writeEntry, align, alignOffset);
    }

    internal static void WriteDataHeaders<T>(this BinaryWriter writer, ArrayProxy<T> obj, Action<T> writeEntry, int? align = null, long alignOffset = 0) {
        obj.WriteDataHeaders(writer, writeEntry, align, alignOffset);
    }
}