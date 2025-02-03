using System.Reflection.Emit;

// ReSharper disable once CheckNamespace
namespace FF7R2.DataObject;

public static class TypeSize<T> {
    // Deliberate as it's designed to change based on the type.
    // ReSharper disable once StaticMemberInGenericType
    public static readonly int Size;

    static TypeSize() {
        var dm = new DynamicMethod("SizeOfType", typeof(int), []);
        var il = dm.GetILGenerator();
        il.Emit(OpCodes.Sizeof, typeof(T));
        il.Emit(OpCodes.Ret);
        Size = (int) (dm.Invoke(null, null) ?? throw new($"Unable to determine size for type: {typeof(T)}"));
    }
}