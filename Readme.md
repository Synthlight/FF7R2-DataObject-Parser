A C# parser for FF7 Rebirth's DataObject files.

# Installation
![NuGet Version](https://img.shields.io/nuget/v/FF7R2.DataObject.API)<br>
[https://www.nuget.org/packages/FF7R2.DataObject.API](https://www.nuget.org/packages/FF7R2.DataObject.API)<br>
Eitehr install the NuGet package, or clone this and add the "DataObject API" project as a project reference to your own project.

# Description
A note about arrays: The game re-uses a arrays and sub-objects wherever possible in the original files.<br>
This doesn't. it can read it either way, but when it writes out the arrays'n'things, it write out a new array each time.

The effects of this haven't been fully tested, but from what I *have* tested, it seem to work without issue.

THIS ONLY WORKS ON THE ORIGINAL IOSTORE FILES!<br>
It will **not** work on Zen extracted uasset/uexp files.

# Save Modes
There's two save modes:
- OG_MODIFIED_BYTES
  - This acts as a proxy to hex editing. Setting a value through `DataAsByteProxy` will directly change bytes at this given offset without writing the whole parsed file.
  - This works, at the cost of immutable array sizes.
- WRITE_PARSED_DATA
  - This tries to write out the parsed file, and whilst not all files pass write tests, does actually work and is the only one to allow changing array sizes.
  - This mode lets you add/remove elements to arrays!

DON'T MIX AND MATCH SAVE TYPES! Don't use `DataAsByteProxy` and then write with `WRITE_PARSED_DATA`. Strange things might happen!<br>
Most likely, though, you'll probably just wind up not saving some change you intend due to one mode ignoring some change from another.

# Examples
An example maximizing materia slots (for `OG_MODIFIED_BYTES`):
```cs
var asset = IoStoreAsset.Load(@"path\to\in\Equipment.uasset");

foreach (var (key, value) in asset.innerAsset.frozenObject.DataTable) {
    value.propertyValues[EquipmentProperties.MateriaSlotDouble]!.As<ByteProperty>()!.DataAsByteProxy = 4;
    value.propertyValues[EquipmentProperties.MateriaSlotSingle]!.As<ByteProperty>()!.DataAsByteProxy = 0;
}

asset.Save(@"path\to\out\Equipment.uasset", IoStoreAsset.Mode.OG_MODIFIED_BYTES);
```
<br>

And another example used to make all spells cast instantly (for `WRITE_PARSED_DATA`):
```cs
var asset = IoStoreAsset.Load(@"in\path\BattleAbility.uasset");
foreach (var (_, value) in asset.innerAsset.frozenObject.DataTable) {
    foreach (var entry in value.propertyValues[BattleAbilityProperties.AnimationParameter_Array]!.As<ArrayPropertyValue>()!.Data!) {
        entry.As<FloatProperty>()!.Data = 0;
    }
}
asset.Save(@"out\path\BattleAbility.uasset", IoStoreAsset.Mode.WRITE_PARSED_DATA);
```

# Row/Column (Property) Names
There are files with string constants named for each DataObject file.<br>
e.g. `EquipmentRows` or `EquipmentProperties` for the file `Equipment.uasset`.

You can reference them anywhere and they are used in the examples.<br>
Very few things are excluded from this and I think those wind up just being some UTF-16/JPN names, and rows that are `None` or `.`.