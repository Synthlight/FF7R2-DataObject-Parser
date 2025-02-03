using System.Runtime.InteropServices;

namespace FF7R2.Enums;

// Copied from CUE4Parse and stripped of the UE5 fields so data can be read directly.
// Also because the OG is readonly, and we need to be able to change it.

[StructLayout(LayoutKind.Sequential)]
public struct FExportBundleHeader {
    public uint FirstEntryIndex;
    public uint EntryCount;
}