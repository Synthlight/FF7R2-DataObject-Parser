using System.Runtime.InteropServices;

namespace FF7R2.Enums;

// Copied from CUE4Parse and added setters.
// Also because the OG is readonly, and we need to be able to change it.

[StructLayout(LayoutKind.Sequential)]
public struct FSerializedNameHeader {
    private byte data0;
    private byte data1;

    public bool IsUtf16 {
        get => (data0 & 0x80u) != 0;
        set {
            if (value) {
                data0 |= (byte) 0x80u;
            } else {
                data0 &= (byte) 0x7Fu;
            }
        }
    }

    public ushort Length {
        get => (ushort) (((data0 & 0x7Fu) << 8) + data1);
        set {
            var dataWithOnlyTheUtf16Bit         = (byte) (data0 & (byte) 0x80u);
            var valueLeftHalfWithoutTheUtf16Bit = (byte) ((value & (ushort) 0x7F00u) >> 8);
            data0 = (byte) (dataWithOnlyTheUtf16Bit | valueLeftHalfWithoutTheUtf16Bit);
            data1 = (byte) (value & (byte) 0xFFu);
        }
    }
}