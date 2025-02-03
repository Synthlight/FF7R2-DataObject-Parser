using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CUE4Parse.UE4.Exceptions;
using CUE4Parse.UE4.Objects.UObject;
using FF7R2.DataObject;

namespace FF7R2;

public static class Extensions {
    public static FName ReadFName(this BinaryReader reader, FName[] names) {
        var index  = reader.ReadInt32();
        var number = reader.ReadInt32();
        var text   = index > names.Length ? "" : names[index].Text;
        return new(text, index, number);
    }

    public static void Write(this BinaryWriter writer, FName name) {
        writer.Write(name.Index);
        writer.Write(name.Number);
    }

    public static T[] ReadArrayProxy<T>(this BinaryReader reader, InnerAsset asset, Func<int, int, T> readEntry, int? align = null) {
        var initialPos = reader.BaseStream.Position;
        var dataPtr    = new MemoryImagePtr(asset);
        dataPtr.Read(reader);
        var arrayNum = reader.ReadInt32();
        var arrayMax = reader.ReadInt32();

        var continuePos = reader.BaseStream.Position;
        reader.BaseStream.Position = initialPos + dataPtr.OffsetFromThis;
        var data = new T[arrayNum];
        for (var i = 0; i < data.Length; i++) {
            data[i] = readEntry(arrayNum, i);
            if (align != null) {
                reader.BaseStream.Position = reader.BaseStream.Position.Align((int) align, asset.frozenObject.frozenObjectStart);
            }
        }
        reader.BaseStream.Position = continuePos;
        return data;
    }

    public static string? ReadFStringProxy(this BinaryReader reader, InnerAsset asset) {
        var initialPos = reader.BaseStream.Position;
        var dataPtr    = new MemoryImagePtr(asset);
        dataPtr.Read(reader);
        var arrayNum = reader.ReadInt32();
        // ReSharper disable once UnusedVariable
        var arrayMax = reader.ReadInt32();
        if (arrayNum <= 1) return null;

        var continuePos = reader.BaseStream.Position;
        reader.BaseStream.Position = initialPos + dataPtr.OffsetFromThis;
        var ucs2Bytes = reader.ReadBytes(arrayNum * 2);
        reader.BaseStream.Position = continuePos;
#if !NO_STRING_NULL_TERMINATION_VALIDATION
        if (ucs2Bytes[^1] != 0 || ucs2Bytes[^2] != 0) throw new ParserException("Serialized FString is not null terminated");
#endif
        return Encoding.Unicode.GetString(ucs2Bytes, 0, ucs2Bytes.Length - 2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Align(this long ptr, int alignment, long offset = 0) {
        ptr -= offset;
        return (ptr + alignment - 1 & ~(alignment - 1)) + offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Align(this int ptr, int alignment, long offset = 0) {
        ptr -= (int) offset;
        return (ptr + alignment - 1 & ~(alignment - 1)) + (int) offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Align(this uint ptr, int alignment, long offset = 0) {
        ptr -= (uint) offset;
        return (ptr + alignment - 1 & ~(alignment - 1)) + offset;
    }

    public static T Read<T>(this BinaryReader reader) {
        var size   = Unsafe.SizeOf<T>();
        var buffer = reader.ReadBytes(size);
        return Unsafe.ReadUnaligned<T>(ref buffer[0]);
    }

    public static T[] Subsequence<T>(this IEnumerable<T> arr, int startIndex, int length) {
        return arr.Skip(startIndex).Take(length).ToArray();
    }

    public static string ReadNullTermString(this BinaryReader reader) {
        var stringBytes = new List<byte>();
        do {
            stringBytes.Add(reader.ReadByte());
        } while (stringBytes[^1] != 0);

        return Encoding.UTF8.GetString(stringBytes.Subsequence(0, stringBytes.Count).ToArray());
    }

    public static T GetDataAs<T>(this IEnumerable<byte> bytes) {
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        try {
            return (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T))!;
        } finally {
            if (handle.IsAllocated) handle.Free();
        }
    }

    public static byte[] GetBytes<T>(this T @struct) {
        var size   = Marshal.SizeOf(@struct);
        var bytes  = new byte[size];
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

        try {
            Marshal.StructureToPtr(@struct!, handle.AddrOfPinnedObject(), false);
            return bytes;
        } finally {
            if (handle.IsAllocated) handle.Free();
        }
    }
}