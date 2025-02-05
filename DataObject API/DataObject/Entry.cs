using FF7R2.DataObject.Properties;

namespace FF7R2.DataObject;

public class Entry(FrozenObject obj, Property[] properties) {
    public readonly Property[]     properties     = properties;
    public          PropertyValues propertyValues = [];

    public void Read(BinaryReader reader) {
        propertyValues = [];
        foreach (var property in properties) {
            if (property.name.ToString().EndsWith("_Array")) {
                var propertyValue = new ArrayPropertyValue(obj, property);
                propertyValue.Read(reader);
                propertyValues.Add(propertyValue);
            } else {
                var propertyValue = property.Create();
                propertyValue.Read(reader);
                propertyValues.Add(propertyValue);
            }
        }
    }

    internal void Write(BinaryWriter writer, PropertyWriteMode mode) {
        foreach (var propertyValue in propertyValues) {
            propertyValue.Write(writer, mode);
        }
    }
}

public static class EntryExtensions {
    internal static void Write(this BinaryWriter writer, Entry obj, PropertyWriteMode mode) {
        obj.Write(writer, mode);
    }
}