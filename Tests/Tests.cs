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
    public void TestReadUserFile(string file) {
        IoStoreAsset.Load(file);
    }

    [DynamicData(nameof(GetFilesToTest), DynamicDataSourceType.Method)]
    [DataTestMethod]
    public void TestWriteUserFile(string file) {
        IoStoreAsset asset;
        try {
            asset = IoStoreAsset.Load(file);
        } catch (Exception e) {
            Assert.Inconclusive($"{e.Message}\n{e.StackTrace}");
            return;
        }
        // Just write to a temp file for easy comparison.
        var tempFile = file.Replace(PathHelper.BASE_PATH, PathHelper.TEST_WRITE_PATH);
        asset.Save(tempFile);
        IoStoreAsset.Load(tempFile);
        /*
        using var memoryStream = new MemoryStream();
        using var writer       = new BinaryWriter(memoryStream);
        try {
            asset.Write(writer);
        } catch (Exception) {
            // Redo but to a temp file output so they can be compared in hex.
            // Should rethrow the same error as `Write`.
            asset.Save(file.Replace(PathHelper.BASE_PATH, PathHelper.TEST_WRITE_PATH));
        }
        */
    }
}