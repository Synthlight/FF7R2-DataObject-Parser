﻿using System.Collections;
using CUE4Parse.Utils;

namespace FF7R2.DataObject;

public class BitArrayData {
    private  MemoryImagePtr dataPtr = new();
    internal BitArray       Data    = new(0);
    private  int            numBits;
    private  int            maxBits;

    private long headerPos;

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
        Data = new(data) {Length = numBits};

        reader.BaseStream.Position = continuePos;
    }

    internal void WriteHeader(BinaryWriter writer) {
        headerPos = writer.BaseStream.Position;
        writer.Write(dataPtr); // Update later.
        writer.Write(numBits);
        writer.Write(maxBits);
    }

    internal void WriteData(BinaryWriter writer, long alignOffset) {
        var initialPos = writer.BaseStream.Position;

        if (numBits == 0) {
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
        writer.Write(dataPtr);

        writer.BaseStream.Position = headerPos + dataPtr.OffsetFromThis;

        var bytes = new byte[numBits.DivideAndRoundUp(8)];
        Data.CopyTo(bytes, 0);
        writer.Write(bytes);
        writer.BaseStream.Position = writer.BaseStream.Position.Align(4, alignOffset);
    }
}

public static class BitArrayDataExtensions {
    internal static void WriteHeader(this BinaryWriter writer, BitArrayData obj) {
        obj.WriteHeader(writer);
    }

    internal static void WriteData(this BinaryWriter writer, BitArrayData obj, long alignOffset) {
        obj.WriteData(writer, alignOffset);
    }
}