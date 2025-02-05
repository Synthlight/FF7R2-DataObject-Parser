namespace FF7R2.DataObject.Properties;

public class PropertyValues : List<PropertyValue> {
    public PropertyValue? this[string key] => this.FirstOrDefault(value => value.property.name.Text == key);
}