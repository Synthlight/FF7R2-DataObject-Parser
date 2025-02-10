namespace FF7R2.DataObject;

public class ArrayProxy<T> {
    private MemoryImagePtr dataPtr = new();
    public  List<T>        data    = [];
    private int            dataMax;

    private long headerPos;

    internal void Read(BinaryReader reader, Func<T> readEntry, int? align = null, long alignOffset = 0) {
        var initialPos = reader.BaseStream.Position;
        dataPtr = new();
        dataPtr.Read(reader);
        var dataNum = reader.ReadInt32();
        dataMax = reader.ReadInt32();

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
        headerPos = writer.BaseStream.Position;
        writer.Write(dataPtr); // Update later.
        writer.Write(data.Count);
        writer.Write(dataMax);
    }

    internal void WriteDataHeaders(BinaryWriter writer, Action<T> writeEntry) {
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
        writer.Write(dataPtr);

        writer.BaseStream.Position = headerPos + dataPtr.OffsetFromThis;

        foreach (var entry in data) {
            writeEntry(entry);
        }
    }

    internal void WriteData(BinaryWriter writer, Action<T> writeEntry, int? align = null, long alignOffset = 0) {
        foreach (var entry in data) {
            writeEntry(entry);
            if (align != null) {
                writer.BaseStream.Position = writer.BaseStream.Position.Align((int) align, alignOffset);
            }
        }
    }
}

public static class ArrayProxyExtensions {
    internal static void WriteHeader<T>(this BinaryWriter writer, ArrayProxy<T> obj) {
        obj.WriteHeader(writer);
    }

    internal static void WriteData<T>(this BinaryWriter writer, ArrayProxy<T> obj, Action<T> writeEntry, int? align = null, long alignOffset = 0) {
        obj.WriteData(writer, writeEntry, align, alignOffset);
    }

    internal static void WriteDataHeaders<T>(this BinaryWriter writer, ArrayProxy<T> obj, Action<T> writeEntry) {
        obj.WriteDataHeaders(writer, writeEntry);
    }
}