using System.Data.HashFunction.CityHash;
using System.Text;
using CUE4Parse.UE4.IO.Objects;
using CUE4Parse.UE4.Objects.UObject;
using FExportBundleHeader = FF7R2.Enums.FExportBundleHeader;
using FExportMapEntry = FF7R2.Enums.FExportMapEntry;
using FPackageSummary = FF7R2.Enums.FPackageSummary;
using FSerializedNameHeader = FF7R2.Enums.FSerializedNameHeader;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace FF7R2.DataObject;

public class IoStoreAsset {
    internal static readonly ICityHash CITY = CityHashFactory.Instance.Create(new CityHashConfig {HashSizeInBits = 64});

    // These are sort-of in the order they appear in the file.
    internal byte[]                bytes;
    private  FPackageSummary       packageSummary;
    internal FName[]               names;
    private  byte[]                startOfHashBytes; // Seems to always be `00 00 64 C1 00 00 00 00` in all the data object files.
    private  FPackageObjectIndex[] imports;
    private  FExportMapEntry[]     exports;
    private  FExportBundleHeader   bundleHeader;
    private  FExportBundleEntry[]  bundles;
    private  FPackageId[]          packages;
    public   InnerAsset            innerAsset;

    private IoStoreAsset(byte[] bytes) {
        this.bytes = bytes;
    }

    public static IoStoreAsset Load(string file) {
        var bytes = File.ReadAllBytes(file);

        using var memoryStream = new MemoryStream();
        memoryStream.Write(bytes);
        memoryStream.Seek(0, SeekOrigin.Begin);

        var       asset  = new IoStoreAsset(bytes);
        using var reader = new BinaryReader(memoryStream);
        asset.Read(reader);

        return asset;
    }

    /// <summary>
    /// There's two modes of saving.
    /// <see cref="Mode.OG_MODIFIED_BYTES"/> works in conjunction with the `DataAsByteProxy` property and just reads/writes to/from the bytes at specific offsets.
    /// This works, at the cost of immutable array sizes.
    /// <see cref="Mode.WRITE_PARSED_DATA"/>
    /// This tries to write out the parsed file, and results are buggy in-game at best.
    /// Tests pass for few files, and the ones that do (like equipment) just result in the items being gone in-game.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="mode"></param>
    public void Save(string file, Mode mode) {
        switch (mode) {
            case Mode.OG_MODIFIED_BYTES: {
                Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                File.WriteAllBytes(file, bytes);
                break;
            }
            case Mode.WRITE_PARSED_DATA: {
                Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                var       stream = File.Open(file, FileMode.Create, FileAccess.Write, FileShare.None);
                using var writer = new BinaryWriter(stream);
                Write(writer);
                break;
            }
            default: throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    internal void Read(BinaryReader reader) {
        // Read header.
        packageSummary = reader.Read<FPackageSummary>();

        // Read names.
        reader.BaseStream.Seek(packageSummary.NameMapNamesOffset, SeekOrigin.Begin);
        var nameCount = packageSummary.NameMapHashesSize / sizeof(ulong) - 1;
        names = new FName[nameCount];
        for (var i = 0; i < nameCount; i++) {
            var nameHeader = reader.Read<FSerializedNameHeader>();

            string text;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (nameHeader.IsUtf16) {
                var stringBytes = reader.ReadBytes(nameHeader.Length * 2);
                text = Encoding.Unicode.GetString(stringBytes);
            } else {
                var stringBytes = reader.ReadBytes(nameHeader.Length);
                text = Encoding.ASCII.GetString(stringBytes);
            }

            names[i] = new(text, i);
        }

        // Read the magic 8 bytes at the start of the hash section.
        // Skip the rest.
        reader.BaseStream.Seek(packageSummary.NameMapHashesOffset, SeekOrigin.Begin);
        startOfHashBytes = reader.ReadBytes(8);

        // Read imports.
        reader.BaseStream.Seek(packageSummary.ImportMapOffset, SeekOrigin.Begin);
        var importCount = (packageSummary.ExportMapOffset - packageSummary.ImportMapOffset) / FPackageObjectIndex.Size;
        imports = new FPackageObjectIndex[importCount];
        for (var i = 0; i < importCount; i++) {
            imports[i] = reader.Read<FPackageObjectIndex>();
        }

        // Read exports.
        reader.BaseStream.Seek(packageSummary.ExportMapOffset, SeekOrigin.Begin);
        var exportCount = (packageSummary.ExportBundlesOffset - packageSummary.ExportMapOffset) / FExportMapEntry.SIZE;
        exports = new FExportMapEntry[exportCount];
        for (var i = 0; i < exportCount; i++) {
            exports[i] = reader.Read<FExportMapEntry>();
        }

        if (exportCount > 1) {
            // TODO: Add support for more than one export.
            throw new NotImplementedException($"{exportCount} exports found. Support for more than one export has not been implemented.");
        }

        // Read bundles.
        reader.BaseStream.Seek(packageSummary.ExportBundlesOffset, SeekOrigin.Begin);
        bundleHeader = reader.Read<FExportBundleHeader>();
        bundles      = new FExportBundleEntry[bundleHeader.EntryCount];
        for (var i = 0; i < bundleHeader.EntryCount; i++) {
            bundles[i] = reader.Read<FExportBundleEntry>();
        }

        // Read packages.
        reader.BaseStream.Seek(packageSummary.GraphDataOffset, SeekOrigin.Begin);
        var packageCount = reader.ReadInt32();
        packages = new FPackageId[packageCount];
        for (var i = 0; i < packageCount; i++) {
            packages[i] = reader.Read<FPackageId>();
            // TODO: I think there's more here. A bundle count, maybe. Not sure what to do with it. Haven't seen a sample yet.
        }

        // Read inner asset.
        reader.BaseStream.Seek(packageSummary.GraphDataOffset + packageSummary.GraphDataSize, SeekOrigin.Begin);
        innerAsset = new(this, exports[0]);
        innerAsset.Read(reader);
    }

    internal void Write(BinaryWriter writer) {
        // Write names.
        writer.BaseStream.Seek(packageSummary.NameMapNamesOffset, SeekOrigin.Begin); // Should always be 64, but I'm just assuming the file has it right to begin with.
        foreach (var name in names) {
            var nameHeader = new FSerializedNameHeader();
            if (name.Text.ContainsUnicodeCharacter()) {
                nameHeader.IsUtf16 = true;
            }
            nameHeader.Length = (byte) name.Text.Length;
            writer.Write(nameHeader);

            byte[] bytes;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (nameHeader.IsUtf16) {
                bytes = Encoding.Unicode.GetBytes(name.Text);
            } else {
                bytes = Encoding.ASCII.GetBytes(name.Text);
            }

            writer.Write(bytes);
        }
        packageSummary.NameMapNamesSize = (int) (writer.BaseStream.Position - packageSummary.NameMapNamesOffset);

        // Write name hashes.
        writer.BaseStream.Position         = writer.BaseStream.Position.Align(8);
        packageSummary.NameMapHashesOffset = (int) writer.BaseStream.Position;
        writer.Write(startOfHashBytes);
        foreach (var name in names) {
            var bytes = Encoding.UTF8.GetBytes(name.Text.ToLower());
            var hash  = CITY.ComputeHash(bytes);
            writer.Write(hash.Hash);
        }
        packageSummary.NameMapHashesSize = (int) (writer.BaseStream.Position - packageSummary.NameMapHashesOffset);

        // Write imports.
        packageSummary.ImportMapOffset = (int) writer.BaseStream.Position;
        foreach (var import in imports) {
            writer.Write(import.GetBytes());
        }

        // Write exports.
        packageSummary.ExportMapOffset = (int) writer.BaseStream.Position;
        foreach (var export in exports) {
            writer.Write(export.GetBytes());
        }

        // Write bundles.
        packageSummary.ExportBundlesOffset = (int) writer.BaseStream.Position;
        writer.Write(bundleHeader.GetBytes());
        foreach (var bundle in bundles) {
            writer.Write(bundle.GetBytes());
        }

        // Write packages.
        packageSummary.GraphDataOffset = (int) writer.BaseStream.Position;
        writer.Write(packages.Length);
        foreach (var package in packages) {
            writer.Write(package.id);
        }

        if (exports.Length > 1) {
            // TODO: Add support for more than one export.
            throw new NotImplementedException($"{exports.Length} exports found. Support for more than one export has not been implemented.");
        }

        // Write the inner assets.
        var innerAssetStart = packageSummary.GraphDataOffset + packageSummary.GraphDataSize;
        writer.BaseStream.Seek(innerAssetStart, SeekOrigin.Begin);
        writer.Write(innerAsset);
        var innerAssetSize = writer.BaseStream.Position - innerAssetStart;
        exports[0].CookedSerialSize = (ulong) innerAssetSize;

        // Update exports.
        writer.BaseStream.Position = packageSummary.ExportMapOffset;
        foreach (var export in exports) {
            writer.Write(export.GetBytes());
        }

        // Finally, update the header.
        writer.BaseStream.Seek(0, SeekOrigin.Begin);
        writer.Write(packageSummary.GetBytes());
    }

    public enum Mode {
        OG_MODIFIED_BYTES,
        WRITE_PARSED_DATA
    }
}