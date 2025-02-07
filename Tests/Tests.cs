using FF7R2.DataObject;
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
        IoStoreAsset.Load(file);
    }

    [DynamicData(nameof(GetFilesToTest), DynamicDataSourceType.Method)]
    [DataTestMethod, Timeout(40000)]
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
        IoStoreAsset.Load(tempFile);
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
        IoStoreAsset.Load(tempFile);
    }
}