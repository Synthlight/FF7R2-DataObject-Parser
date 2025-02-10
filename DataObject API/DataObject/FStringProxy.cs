﻿using System.Text;

namespace FF7R2.DataObject;

public class FStringProxy {
    private  MemoryImagePtr dataPtr = new();
    internal string?        data;
    private  int            charMax;

    private long headerPos;

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
        headerPos = writer.BaseStream.Position;
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
        writer.Write(dataPtr);

        writer.BaseStream.Position = headerPos + dataPtr.OffsetFromThis;

        var bytes = Encoding.Unicode.GetBytes(data!);
        Array.Resize(ref bytes, bytes.Length + 2);
        writer.Write(bytes);
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