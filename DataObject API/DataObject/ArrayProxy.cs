namespace FF7R2.DataObject;

public class ArrayProxy<T> {
    private  MemoryImagePtr dataPtr = new();
    internal T[]            data    = [];
    private  int            dataMax;

    internal void Read(BinaryReader reader, Func<T> readEntry, int? align = null, long alignOffset = 0) {
        var initialPos = reader.BaseStream.Position;
        dataPtr = new();
        dataPtr.Read(reader);
        var dataNum = reader.ReadInt32();
        dataMax = reader.ReadInt32();

        if (dataNum == 0) return;

        var continuePos = reader.BaseStream.Position;
        reader.BaseStream.Position = initialPos + dataPtr.OffsetFromThis;

        var data = new List<T>(dataNum);
        for (var i = 0; i < dataNum; ++i) {
            data.Add(readEntry());
            if (align != null) {
                reader.BaseStream.Position = reader.BaseStream.Position.Align((int) align, alignOffset);
            }
        }
        this.data = data.ToArray();

        reader.BaseStream.Position = continuePos;
    }

    internal void WriteHeader(BinaryWriter writer) {
        writer.Write(dataPtr); // Update later.
        writer.Write(data.Length);
        writer.Write(dataMax);
    }

    internal void WriteData(BinaryWriter writer, long headerPos, Action<T> writeEntry, int? align = null, long alignOffset = 0) {
        if (data.Length == 0) return;

        var initialPos = writer.BaseStream.Position;

        writer.BaseStream.Position = headerPos;
        dataPtr.OffsetFromThis     = initialPos - headerPos;
        writer.Write(dataPtr);

        writer.BaseStream.Position = headerPos + dataPtr.OffsetFromThis;

        foreach (var entry in data) {
            writeEntry(entry);
            if (align != null) {
                writer.BaseStream.Position = writer.BaseStream.Position.Align((int) align, alignOffset);
            }
        }
    }
}

public static class ArrayProxyExtensions {
    public static void WriteHeader<T>(this BinaryWriter writer, ArrayProxy<T> obj) {
        obj.WriteHeader(writer);
    }

    public static void WriteData<T>(this BinaryWriter writer, ArrayProxy<T> obj, long headerPos, Action<T> writeEntry, int? align = null, int alignOffset = 0) {
        obj.WriteData(writer, headerPos, writeEntry, align, alignOffset);
    }
}