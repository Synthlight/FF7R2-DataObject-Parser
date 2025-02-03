using System.Collections;
using CUE4Parse.Utils;

namespace FF7R2.DataObject;

public class BitArrayData {
    private  MemoryImagePtr dataPtr = new();
    internal BitArray       Data    = new(0);
    private  int            numBits;
    private  int            maxBits;

    internal void Read(BinaryReader reader) {
        var initialPos = reader.BaseStream.Position;
        dataPtr = new();
        dataPtr.Read(reader);
        numBits = reader.ReadInt32();
        maxBits = reader.ReadInt32();

        if (numBits == 0) return;

        var continuePos = reader.BaseStream.Position;
        reader.BaseStream.Position = initialPos + dataPtr.OffsetFromThis;

        var data = reader.ReadBytes(numBits.DivideAndRoundUp(8));
        reader.BaseStream.Position = continuePos;
        Data                       = new(data) {Length = numBits};

        reader.BaseStream.Position = continuePos;
    }

    internal void WriteHeader(BinaryWriter writer) {
        writer.Write(dataPtr); // Update later.
        writer.Write(numBits);
        writer.Write(maxBits);
    }

    internal void WriteData(BinaryWriter writer, long headerPos) {
        if (numBits == 0) return;

        var initialPos = writer.BaseStream.Position;

        writer.BaseStream.Position = headerPos;
        dataPtr.OffsetFromThis     = initialPos - headerPos;
        writer.Write(dataPtr);

        writer.BaseStream.Position = headerPos + dataPtr.OffsetFromThis;

        var bytes = new byte[numBits.DivideAndRoundUp(8)];
        Data.CopyTo(bytes, 0);
        writer.Write(bytes);
    }
}

public static class BitArrayDataExtensions {
    public static void WriteHeader(this BinaryWriter writer, BitArrayData obj) {
        obj.WriteHeader(writer);
    }

    public static void WriteData(this BinaryWriter writer, BitArrayData obj, long headerPos) {
        obj.WriteData(writer, headerPos);
    }
}