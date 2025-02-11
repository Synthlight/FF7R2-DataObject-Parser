using System.Diagnostics;
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

        var data = new List<byte> {0, 1, 2, 3, 4, 5, 6, 7};
        Debug.Assert(data.GetListHashCode() != 0);
    }

    [TestMethod]
    public void TestAssignArrayData() {
        var asset     = IoStoreAsset.Load(@"V:\FF7R2\End\Content\DataObject\Resident\Equipment.uasset");
        var equipment = asset.innerAsset.frozenObject.DataTable[EquipmentRows.E_ACC_1002];
        var prop      = equipment.propertyValues[EquipmentProperties.StatusChangeResist_Array];
        if (prop == null) {
            throw new("`prop` is null.");
        }

        var accessory = asset.innerAsset.frozenObject.DataTable[EquipmentRows.E_ACC_0008]; // E_ACC_0001, E_ACC_0003
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
        var asset       = IoStoreAsset.Load(@"V:\FF7R2\End\Content\DataObject\Resident\Equipment.uasset");
        var equipment   = asset.innerAsset.frozenObject.DataTable[EquipmentRows.E_ACC_1002];
        var ogArrayProp = equipment.propertyValues[EquipmentProperties.StatusChangeResist_Array]?.As<ArrayProperty>();
        if (ogArrayProp == null) {
            throw new("`prop` is null.");
        }

        for (var i = 1; i <= 9999; i++) {
            var key = $"E_ACC_{i:D4}";
            if (i is 1 or 3 or 214 or 256) {
                Debug.WriteLine($"Skipping acc: {key}");
                continue;
            }
            if (key == nameof(EquipmentRows.E_ACC_1002)) continue;
            if (!asset.innerAsset.frozenObject.DataTable.TryGetValue(key, out var accessory)) continue;
            Debug.WriteLine($"Changing acc: {key}");
            var arrayProp = accessory.propertyValues[EquipmentProperties.StatusChangeResist_Array]!.As<ArrayProperty>()!;
            /*
            if (ogArrayProp.data.dataPtr.isFrozen != arrayProp.data.dataPtr.isFrozen) {
                throw new("`isFrozen` doesn't match.");
            }
            */
            arrayProp.Data = ogArrayProp.Data;
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