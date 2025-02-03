using System.Text;

namespace FF7R2.DataObject;

public class FStringProxy {
    private  MemoryImagePtr dataPtr = new();
    internal string?        data;
    private  int            charMax;

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
        writer.Write(dataPtr); // Update later.
        writer.Write(data?.Length ?? 0);
        writer.Write(charMax);
    }

    internal void WriteData(BinaryWriter writer, long headerPos) {
        if (data == null || data?.Length == 0) return;

        var initialPos = writer.BaseStream.Position;

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

    internal static void WriteData(this BinaryWriter writer, FStringProxy obj, long headerPos) {
        obj.WriteData(writer, headerPos);
    }
}