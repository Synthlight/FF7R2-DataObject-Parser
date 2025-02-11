namespace FF7R2.DataObject;

internal readonly struct CachedObject(long absoluteDataOffset, ICachableObject? obj) : IEquatable<CachedObject> {
    public readonly long             absoluteDataOffset = absoluteDataOffset;
    public readonly ICachableObject? obj                = obj;

    public override int GetHashCode() {
        return (obj != null ? obj.GetCacheHash() : 0);
    }

    public override string? ToString() {
        return obj?.ToString();
    }

    public bool Equals(CachedObject other) {
        return Equals(obj, other.obj);
    }

    public override bool Equals(object? obj) {
        return obj is CachedObject other && Equals(other);
    }

    public static bool operator ==(CachedObject left, CachedObject right) {
        return left.Equals(right);
    }

    public static bool operator !=(CachedObject left, CachedObject right) {
        return !left.Equals(right);
    }
}

internal interface ICachableObject {
    int GetCacheHash();
}