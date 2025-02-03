using FF7R2.DataObject.Properties;

namespace FF7R2.DataObject;

public class Entry(FrozenObject obj, Property[] properties) {
    public readonly Property[]     properties = properties;
    public          PropertyValues propertyValues;

    public void Read(BinaryReader reader) {
        propertyValues = [];
        foreach (var property in properties) {
            if (property.Name.ToString().EndsWith("_Array")) {
                var propertyValue = new Ff7R2ArrayPropertyValue(obj, property);
                propertyValue.Read(reader);
                propertyValues.Add(propertyValue);
            } else {
                var propertyValue = property.Create();
                propertyValue.Read(reader);
                propertyValues.Add(propertyValue);
            }
        }
    }

    public class PropertyValues : List<PropertyValue> {
        public PropertyValue? this[string key] => this.FirstOrDefault(value => value.property.Name.Text == key);
    }
}