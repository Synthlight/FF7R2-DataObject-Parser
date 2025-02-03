namespace FF7R2.DataObject;

public class SparseArrayProxy<T> {
    private  MemoryImagePtr dataPtr = new();
    internal T[]            data    = [];
    private  int            dataMax;
    private  BitArrayData   allocationFlags = new();
    private  int            firstFreeIndex;
    private  int            numFreeIndices;

    private long headerPos;
    private long allocationFlagsPos;

    internal void Read(BinaryReader reader, Func<T> readEntry) {
        var initialPos = reader.BaseStream.Position;
        dataPtr = new();
        dataPtr.Read(reader);
        var dataNum = reader.ReadInt32();
        dataMax         = reader.ReadInt32();
        allocationFlags = new();
        allocationFlags.Read(reader);
        firstFreeIndex = reader.ReadInt32();
        numFreeIndices = reader.ReadInt32();

        if (dataNum != dataMax) {
            throw new NotImplementedException("dataNum != dataMax");
        }
        if (dataNum == 0) return;

        var continuePos = reader.BaseStream.Position;
        reader.BaseStream.Position = initialPos + dataPtr.OffsetFromThis;

        var data = new List<T>(dataNum);
        for (var i = 0; i < dataNum; ++i) {
            // Read anyways for the purpose of writing out the same data later.
            //if (allocationFlags[i]) {
            data.Add(readEntry());
            //}
        }
        this.data = data.ToArray();

        reader.BaseStream.Position = continuePos;
    }

    internal void WriteHeader(BinaryWriter writer) {
        headerPos = writer.BaseStream.Position;
        writer.Write(dataPtr); // Update later.
        writer.Write(data.Length);
        writer.Write(dataMax);
        allocationFlagsPos = writer.BaseStream.Position;
        writer.WriteHeader(allocationFlags);
        writer.Write(firstFreeIndex);
        writer.Write(numFreeIndices);
    }

    internal void WriteData(BinaryWriter writer, Action<T> writeEntry) {
        if (data.Length == 0) return;

        var initialPos = writer.BaseStream.Position;

        if (headerPos == 0) throw new("Header position not set.");
        writer.BaseStream.Position = headerPos;
        dataPtr.OffsetFromThis     = initialPos - headerPos;
        writer.Write(dataPtr);

        writer.BaseStream.Position = headerPos + dataPtr.OffsetFromThis;

        foreach (var entry in data) {
            // Write anyways for the purpose of writing out the same data we read.
            //if (allocationFlags[i]) {
            writeEntry(entry);
            //}
        }

        // Update the offset bitfield data too.
        writer.WriteData(allocationFlags);
    }
}

public static class SparseArrayProxyExtensions {
    internal static void WriteHeader<T>(this BinaryWriter writer, SparseArrayProxy<T> obj) {
        obj.WriteHeader(writer);
    }

    internal static void WriteData<T>(this BinaryWriter writer, SparseArrayProxy<T> obj, Action<T> writeEntry) {
        obj.WriteData(writer, writeEntry);
    }
}