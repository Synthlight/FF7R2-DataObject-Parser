﻿using CUE4Parse.UE4.Assets.Exports;
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
    private int          someBool;
    public  FrozenObject frozenObject = null!;

    // ReSharper disable ParameterHidesMember
    internal void Read(BinaryReader reader) {
        tag = reader.ReadFName(ioStoreAsset.names);

        if (tag.Text != "None") {
            // TODO: Add support for reading tags.
            throw new NotImplementedException("Inner asset tags found. Not supported yet.");
        }

        someBool = reader.ReadInt32();
        if (!export.ObjectFlags.HasFlag(EObjectFlags.RF_ClassDefaultObject) && someBool == 1) {
            // TODO: Add support for GUIDs.
            throw new NotImplementedException("Inner asset GUID found. Not supported yet.");
        }

        frozenObject = new(this);
        frozenObject.Read(reader);
    }

    internal void Write(BinaryWriter writer) {
        // TODO: Add tag support.
        writer.Write(tag);
        writer.Write(someBool);

        writer.Write(frozenObject);
    }
}

public static class InnerAssetExtensions {
    internal static void Write(this BinaryWriter writer, InnerAsset asset) {
        asset.Write(writer);
    }
}