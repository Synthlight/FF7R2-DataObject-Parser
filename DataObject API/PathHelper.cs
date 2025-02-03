namespace FF7R2;

public static class PathHelper {
    public const string BASE_PATH       = @"V:\FF7R2";
    public const string TEST_WRITE_PATH = @"O:\Temp";

    public static readonly string[] ASSET_PATHS = [
        $@"{BASE_PATH}\End\Content\DataObject\Resident"
    ];

    public static List<string> GetFileList() {
        return (from path in ASSET_PATHS
                where Directory.Exists(path)
                from file in Directory.EnumerateFiles(path, "*.uasset", SearchOption.AllDirectories)
                where File.Exists(file)
                select file).ToList();
    }
}