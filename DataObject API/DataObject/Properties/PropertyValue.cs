namespace FF7R2.DataObject.Properties;

public abstract class PropertyValue(FrozenObject obj, Property property) {
    public readonly FrozenObject obj      = obj;
    public readonly Property     property = property;

    protected abstract object? GenericData { get; }
    public             long    Offset      { get; set; }

    public abstract void Read(BinaryReader  reader);
    public abstract void Write(BinaryWriter writer);

    public T? As<T>() where T : class {
        return this as T;
    }
}

public abstract class PropertyValue<T>(FrozenObject obj, Property property) : PropertyValue(obj, property) {
    protected abstract T?      Data        { get; set; }
    protected override object? GenericData => Data;

    public virtual T? PublicData {
        get {
            var length = TypeSize<T>.Size;
            var buffer = new byte[length];
            Array.Copy(obj.asset.ioStoreAsset.bytes, Offset, buffer, 0, length);
            return buffer.GetDataAs<T>();
        }
        set {
            var bytes = value.GetBytes();
            Array.Copy(bytes, 0, obj.asset.ioStoreAsset.bytes, Offset, TypeSize<T>.Size);
        }
    }

    public override string ToString() =>
        PublicData != null
            ? $"{property.Name}: {PublicData.ToString()} ({property.UnderlyingType}) ------ {Offset}"
            : $"{property.Name}: (null) ({property.UnderlyingType}) ------ {Offset}";
}