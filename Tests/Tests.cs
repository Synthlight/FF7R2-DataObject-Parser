using System.Diagnostics;
using CUE4Parse.GameTypes.FF7.Assets.Objects.Properties;
using FF7R2.DataObject;
using FF7R2.DataObject.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FF7R2.Tests;

[TestClass]
public sealed class Tests {
    private static IEnumerable<object[]> GetFilesToTest() {
        return PathHelper.GetFileList().Select(s => new object[] {s});
    }

    [DynamicData(nameof(GetFilesToTest), DynamicDataSourceType.Method)]
    [DataTestMethod]
    public void TestReadFiles(string file) {
        var asset = IoStoreAsset.Load(file);
        Debug.Assert(asset != null);
    }

    [DynamicData(nameof(GetFilesToTest), DynamicDataSourceType.Method)]
    [DataTestMethod]
    public void CheckForEmptyVsNullStrings(string file) {
        IoStoreAsset asset;
        try {
            asset = IoStoreAsset.Load(file);
        } catch (Exception e) {
            Assert.Inconclusive($"{e.Message}\n{e.StackTrace}");
            return;
        }

        foreach (var (_, value) in asset.innerAsset.frozenObject.DataTable) {
            foreach (var propertyValue in value.propertyValues) {
                if (propertyValue is StrProperty strProp) {
                    if (strProp.Data == "") throw new("Empty string found.");
                } else if (propertyValue is ArrayProperty {property.UnderlyingType: FF7propertyType.StrProperty} arrProp) {
                    foreach (var arrData in arrProp.Data!) {
                        var data = (StrProperty) arrData;
                        if (data.Data == "") throw new("Empty string found.");
                    }
                }
            }
        }
    }

    [DynamicData(nameof(GetFilesToTest), DynamicDataSourceType.Method)]
    [DataTestMethod, Timeout(60000)]
    public void TestWriteThenReadParsedFiles(string file) {
        IoStoreAsset asset;
        try {
            asset = IoStoreAsset.Load(file);
        } catch (Exception e) {
            Assert.Inconclusive($"{e.Message}\n{e.StackTrace}");
            return;
        }
        // Just write to a temp file for easy comparison.
        var tempFile = file.Replace(PathHelper.BASE_PATH, PathHelper.TEST_WRITE_PATH);
        asset.Save(tempFile, IoStoreAsset.Mode.WRITE_PARSED_DATA);

        asset = IoStoreAsset.Load(tempFile);
        Debug.Assert(asset != null);
    }

    [DynamicData(nameof(GetFilesToTest), DynamicDataSourceType.Method)]
    [DataTestMethod]
    public void TestWriteFileBytes(string file) {
        IoStoreAsset asset;
        try {
            asset = IoStoreAsset.Load(file);
        } catch (Exception e) {
            Assert.Inconclusive($"{e.Message}\n{e.StackTrace}");
            return;
        }
        // Just write to a temp file for easy comparison.
        var tempFile = file.Replace(PathHelper.BASE_PATH, PathHelper.TEST_WRITE_PATH);
        asset.Save(tempFile, IoStoreAsset.Mode.OG_MODIFIED_BYTES);

        asset = IoStoreAsset.Load(tempFile);
        Debug.Assert(asset != null);
    }
}