using System.Runtime.InteropServices;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.IO.Objects;

namespace FF7R2.Enums;

// Copied from CUE4Parse and stripped of the UE5 fields so data can be read directly.

[StructLayout(LayoutKind.Sequential)]
public struct FExportMapEntry {
    public ulong               CookedSerialOffset;
    public ulong               CookedSerialSize;
    public FMappedName         ObjectName;
    public FPackageObjectIndex OuterIndex;
    public FPackageObjectIndex ClassIndex;
    public FPackageObjectIndex SuperIndex;
    public FPackageObjectIndex TemplateIndex;
    public FPackageObjectIndex GlobalImportIndex;
    public EObjectFlags        ObjectFlags;
    public uint                FilterFlags; // EExportFilterFlags: client/server flags
}