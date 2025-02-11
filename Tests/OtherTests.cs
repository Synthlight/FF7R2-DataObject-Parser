﻿using System.Diagnostics;
using CUE4Parse.UE4.Objects.UObject;
using FF7R2.Constants;
using FF7R2.DataObject;
using FF7R2.DataObject.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FF7R2.Tests;

[TestClass]
public sealed class OtherTests {
    [TestMethod]
    public void TestListHashes() {
        var a = new List<FName> {new("test", 2, 5)};
        var b = new List<FName> {new("test", 2, 5)};

        Debug.Assert(a[0].GetHashCode() == b[0].GetHashCode());
        Debug.Assert(a.GetListHashCode() == b.GetListHashCode());

        // Using `GetHashCode` by itself wound up with this being 0. Adding the prime multiplier fixed it.
        var data = new List<byte> {0, 1, 2, 3, 4, 5, 6, 7};
        Debug.Assert(data.GetListHashCode() != 0);
    }

    [TestMethod]
    public void TestAssignArrayData() {
        var asset     = IoStoreAsset.Load(@"V:\FF7R2\End\Content\DataObject\Resident\Equipment.uasset");
        var equipment = asset.innerAsset.frozenObject.DataTable[EquipmentRows.E_ACC_1002];
        var prop      = equipment.propertyValues[EquipmentProperties.StatusChangeResist_Array]!;
        var accessory = asset.innerAsset.frozenObject.DataTable[EquipmentRows.E_ACC_0001];
        accessory.propertyValues[EquipmentProperties.StatusChangeResist_Array]!.As<ArrayProperty>()!.Data = prop.As<ArrayProperty>()!.Data;

        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".Equipment.uasset");
        //const string tempFile = @"R:\Games\Final Fantasy VII Remake Rebirth\Mods\GMod\Content\End\Content\DataObject\Resident\Equipment.uasset";
        Debug.WriteLine($"Writing temp file: {tempFile}");
        asset.Save(tempFile, IoStoreAsset.Mode.WRITE_PARSED_DATA);

        asset = IoStoreAsset.Load(tempFile);
        Debug.Assert(asset != null);
    }

    [TestMethod]
    public void TestAssignArrayDataLoop() {
        var asset          = IoStoreAsset.Load(@"V:\FF7R2\End\Content\DataObject\Resident\Equipment.uasset");
        var ribbon         = asset.innerAsset.frozenObject.DataTable[EquipmentRows.E_ACC_1002];
        var ribbonStatuses = ribbon.propertyValues[EquipmentProperties.StatusChangeResist_Array]!.As<ArrayProperty>()!;

        // Apply ribbon resistances to everything!
        foreach (var (key, value) in asset.innerAsset.frozenObject.DataTable) {
            if (key == nameof(EquipmentRows.E_ACC_1002)) continue; // Skip self.
            Debug.WriteLine($"Changing: {key}");
            var arrayProp = value.propertyValues[EquipmentProperties.StatusChangeResist_Array]!.As<ArrayProperty>()!;
            arrayProp.Data = ribbonStatuses.Data;
        }

        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".Equipment.uasset");
        //const string tempFile = @"R:\Games\Final Fantasy VII Remake Rebirth\Mods\GMod\Content\End\Content\DataObject\Resident\Equipment.uasset";
        Debug.WriteLine($"Writing temp file: {tempFile}");
        asset.Save(tempFile, IoStoreAsset.Mode.WRITE_PARSED_DATA);

        asset = IoStoreAsset.Load(tempFile);
        Debug.Assert(asset != null);
    }

    [TestMethod]
    public void TestAddArrayData() {
        var asset         = IoStoreAsset.Load(@"V:\FF7R2\End\Content\DataObject\Resident\Equipment.uasset");
        var accessory     = asset.innerAsset.frozenObject.DataTable[EquipmentRows.E_ACC_0001];
        var propertyValue = accessory.propertyValues[EquipmentProperties.StatusChangeResist_Array]!;
        var arrayData     = propertyValue.As<ArrayProperty>()!.Data!;
        arrayData.Add(new ByteProperty(asset.innerAsset.frozenObject, propertyValue.property));

        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".Equipment.uasset");
        //const string tempFile = @"R:\Games\Final Fantasy VII Remake Rebirth\Mods\GMod\Content\End\Content\DataObject\Resident\Equipment.uasset";
        Debug.WriteLine($"Writing temp file: {tempFile}");
        asset.Save(tempFile, IoStoreAsset.Mode.WRITE_PARSED_DATA);

        asset = IoStoreAsset.Load(tempFile);
        Debug.Assert(asset != null);
    }
}