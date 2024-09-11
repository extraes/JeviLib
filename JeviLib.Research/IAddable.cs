using System.Runtime.Versioning;

[assembly: RequiresPreviewFeatures]

namespace JeviLib.Research;

public interface IAddable<T> where T : IAddable<T>
{
    static abstract T Zero { get; }
    static abstract T operator +(T lhs, T rhs);
}

public record struct MyStruct(float Value) : IAddable<MyStruct>
{
    public static MyStruct Zero => new MyStruct(0);
    public static MyStruct operator +(MyStruct lhs, MyStruct rhs) => new MyStruct(lhs.Value + rhs.Value);
}