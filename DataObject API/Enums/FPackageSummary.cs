using System.Runtime.InteropServices;
using CUE4Parse.UE4.IO.Objects;
using CUE4Parse.UE4.Objects.UObject;

namespace FF7R2.Enums;

// Copied from CUE4Parse and stripped of the UE5 fields so data can be read directly.
// Also because the OG is readonly, and we need to be able to change it.

[StructLayout(LayoutKind.Sequential)]
public struct FPackageSummary {
    public  FMappedName   Name;
    public  FMappedName   SourceName;
    public  EPackageFlags PackageFlags;
    public  uint          CookedHeaderSize;
    public  int           NameMapNamesOffset;
    public  int           NameMapNamesSize;
    public  int           NameMapHashesOffset;
    public  int           NameMapHashesSize;
    public  int           ImportMapOffset;
    public  int           ExportMapOffset;
    public  int           ExportBundlesOffset;
    public  int           GraphDataOffset;
    public  int           GraphDataSize;
    private int           _pad;
}