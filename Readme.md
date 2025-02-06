A C# parser for FF7 Rebirth's DataObject files.

# Installation
![NuGet Version](https://img.shields.io/nuget/v/FF7R2.DataObject.API)<br>
[https://www.nuget.org/packages/FF7R2.DataObject.API](https://www.nuget.org/packages/FF7R2.DataObject.API)<br>
Eitehr install the NuGet package, or clone this and add the "DataObject API" project as a project reference to your own project.

# Description
To say this works would be a massive overstatement.<br>
There's layers and layers of compression and re-use here, and whilst this can *read* the files, writing is experimental at best.

There's two save modes. I'm just gonna copy the doc because I'm lazy:
```
/// <see cref="Mode.OG_MODIFIED_BYTES"/> works in conjunction with the `DataAsByteProxy` property and just reads/writes to/from the bytes at specific offsets.
/// This works, at the cost of immutable array sizes.
/// <see cref="Mode.WRITE_PARSED_DATA"/>
/// This tries to write out the parsed file, and results are buggy in-game at best.
/// Tests pass for few files, and the ones that do (like equipment) just result in the items being gone in-game.
```

Example usage (for the edit-via-bytes method, though switching is fairly simple):
```cs
var asset = IoStoreAsset.Load(@"path\to\in\Equipment.uasset");

foreach (var (key, value) in asset.innerAsset.frozenObject.DataTable) {
    value.propertyValues[EquipmentProperties.MateriaSlotDouble]!.As<ByteProperty>()!.DataAsByteProxy = 4;
    value.propertyValues[EquipmentProperties.MateriaSlotSingle]!.As<ByteProperty>()!.DataAsByteProxy = 0;
}

asset.Save(@"path\to\out\Equipment.uasset", IoStoreAsset.Mode.OG_MODIFIED_BYTES);
```

And another example used to make all spells cast instantly:
```cs
var asset = IoStoreAsset.Load(@"in\path\BattleAbility.uasset");
foreach (var (_, value) in asset.innerAsset.frozenObject.DataTable) {
    foreach (var entry in value.propertyValues[BattleAbilityProperties.AnimationParameter_Array]!.As<ArrayPropertyValue>()!.Data!) {
        entry.As<FloatProperty>()!.DataAsByteProxy = 0;
    }
}
asset.Save(@"out\path\BattleAbility.uasset", IoStoreAsset.Mode.OG_MODIFIED_BYTES);
```