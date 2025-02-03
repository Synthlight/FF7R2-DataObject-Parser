using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Objects.UObject;
using FExportMapEntry = FF7R2.Enums.FExportMapEntry;

namespace FF7R2.DataObject;

/// <summary>
/// The object inside the DataObject export.
/// </summary>
/// <param name="ioStoreAsset"></param>
/// <param name="offset">Required as reading/writing needs alignment to the start of the export object.</param>
public class InnerAsset(IoStoreAsset ioStoreAsset, FExportMapEntry export) {
    internal readonly IoStoreAsset ioStoreAsset = ioStoreAsset;

    private FName        tag;
    public  FrozenObject frozenObject;

    // ReSharper disable ParameterHidesMember
    public void Read(BinaryReader reader) {
        tag = reader.ReadFName(ioStoreAsset.names);

        if (tag.Text != "None") {
            // TODO: Add support for reading tags.
            throw new NotImplementedException("Inner asset tags found. Not supported yet.");
        }

        if (!export.ObjectFlags.HasFlag(EObjectFlags.RF_ClassDefaultObject) && reader.ReadInt32() == 1) {
            // TODO: Add support for GUIDs.
            throw new NotImplementedException("Inner asset GUID found. Not supported yet.");
        }

        frozenObject = new(this);
        frozenObject.Read(reader);
    }

    public void Write(BinaryWriter writer) {
        // TODO: Add tag support.
        writer.Write(tag);

        writer.Write(frozenObject);
    }
}

public static class InnerAssetExtensions {
    public static void Write(this BinaryWriter writer, InnerAsset asset) {
        asset.Write(writer);
    }
}