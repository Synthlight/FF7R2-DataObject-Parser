using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CUE4Parse.UE4.Objects.UObject;

namespace FF7R2;

public static class Extensions {
    public static FName ReadFName(this BinaryReader reader, List<FName> names) {
        var index  = reader.ReadInt32();
        var number = reader.ReadInt32();
        var text   = index > names.Count ? "" : names[index].Text;
        return new(text, index, number);
    }

    public static void Write(this BinaryWriter writer, FName name) {
        writer.Write(name.Index);
        writer.Write(name.Number);
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

    public static T Read<T>(this BinaryReader reader) where T : struct {
        var size   = Unsafe.SizeOf<T>();
        var buffer = reader.ReadBytes(size);
        return Unsafe.ReadUnaligned<T>(ref buffer[0]);
    }

    public static void Write<T>(this BinaryWriter writer, T obj) where T : struct {
        var size   = Unsafe.SizeOf<T>();
        var buffer = new byte[size];
        Unsafe.WriteUnaligned(ref buffer[0], obj);
        writer.Write(buffer);
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

    public static bool ContainsUnicodeCharacter(this string input) {
        const int maxAnsiCode = 255;
        return input.Any(c => c > maxAnsiCode);
    }
}