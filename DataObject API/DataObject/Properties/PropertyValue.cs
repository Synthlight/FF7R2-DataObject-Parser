namespace FF7R2.DataObject.Properties;

public abstract class PropertyValue(FrozenObject obj, Property property) {
    protected readonly FrozenObject obj      = obj;
    public readonly    Property     property = property;

    protected abstract object? GenericData { get; }
    protected internal long    Offset      { get; protected set; }

    internal abstract void Read(BinaryReader  reader);
    internal abstract void Write(BinaryWriter writer, PropertyWriteMode mode);

    public T? As<T>() where T : class {
        return this as T;
    }
}

public abstract class PropertyValue<T>(FrozenObject obj, Property property) : PropertyValue(obj, property) {
    public abstract    T?      Data        { get; set; }
    protected override object? GenericData => Data;

    public virtual T? DataAsByteProxy {
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
        DataAsByteProxy != null
            ? $"{property.name}: {DataAsByteProxy.ToString()} ({property.UnderlyingType}) ------ {Offset}"
            : $"{property.name}: (null) ({property.UnderlyingType}) ------ {Offset}";
}