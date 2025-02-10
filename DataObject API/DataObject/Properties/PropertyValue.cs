namespace FF7R2.DataObject.Properties;

public abstract class PropertyValue(FrozenObject obj, Property property) {
    protected readonly FrozenObject obj      = obj;
    public readonly    Property     property = property;

    public abstract    object? GenericData { get; set; }
    protected internal long    Offset      { get; protected set; }
    public virtual     bool    IsArray     => false;

    internal abstract void Read(BinaryReader  reader);
    internal abstract void Write(BinaryWriter writer, PropertyWriteMode mode);

    public T? As<T>() where T : class {
        return (T) (object) this;
    }
}

public abstract class PropertyValue<T>(FrozenObject obj, Property property) : PropertyValue(obj, property) {
    // ReSharper disable once MemberCanBeProtected.Global
    public abstract T? Data { get; set; }
    public override object? GenericData {
        get => Data;
        set => Data = (T?) value;
    }

    /// <summary>
    /// This acts as a proxy to hex editing. Setting a value through this will directly change bytes at this given offset without writing the whole parsed file.
    /// This requires you save with <see cref="IoStoreAsset.Mode.OG_MODIFIED_BYTES"/> and don't mix/match with <see cref="IoStoreAsset.Mode.WRITE_PARSED_DATA"/>!
    /// Use one or the other!
    /// </summary>
    public virtual T? DataAsByteProxy {
        get {
            var length = TypeSize<T>.Size;
            var buffer = new byte[length];
            Array.Copy(obj.asset.ioStoreAsset.bytes, Offset, buffer, 0, length);
            return buffer.GetDataAs<T>();
        }
        // ReSharper disable once UnusedMember.Global
        set {
            Data = value;
            var bytes = value.GetBytes();
            Array.Copy(bytes, 0, obj.asset.ioStoreAsset.bytes, Offset, TypeSize<T>.Size);
        }
    }

    public override string ToString() =>
        DataAsByteProxy != null
            ? $"{property.name}: {DataAsByteProxy.ToString()} ({property.UnderlyingType}) ------ {Offset}"
            : $"{property.name}: (null) ({property.UnderlyingType}) ------ {Offset}";
}