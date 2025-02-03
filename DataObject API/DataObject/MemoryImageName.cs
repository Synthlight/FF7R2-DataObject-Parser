using CUE4Parse.UE4.Objects.UObject;

namespace FF7R2.DataObject;

public class MemoryImageName(InnerAsset asset) {
    public FName                      name;
    public List<MemoryImageNamePatch> patches;

    internal void Read(BinaryReader reader) {
        name = reader.ReadFName(asset.ioStoreAsset.names);
        var count = reader.ReadUInt32();
        patches = [];
        for (var i = 0; i < count; i++) {
            var patch = new MemoryImageNamePatch(asset);
            patch.Read(reader);
            patches.Add(patch);
        }
    }

    internal void Write(BinaryWriter writer) {
        // TODO
        throw new NotImplementedException();
    }

    public override string ToString() {
        return name.ToString();
    }
}