using Mediator.Benchmarks.Large.Types;

namespace Mediator.Benchmarks.Large.Util;

public readonly struct TypeDict
{
    public TypeDict()
    {
        var dictionary = new Dictionary<TypeKey, object>()
        {
            [new TypeKey(typeof(Request0))] = (object)new Request0Handler().Handle,
        };
    }
}

public readonly struct TypeKey : IEquatable<TypeKey>, IEquatable<Type>
{
    public Type Type { get; }

    public TypeKey(Type type)
    {
        Type = type;
    }

    public bool Equals(TypeKey other) => ReferenceEquals(Type, other.Type);

    public bool Equals(Type type) => ReferenceEquals(Type, type);

    public override bool Equals(object obj)
    {
        if (obj is TypeKey key)
            return Equals(key);
        else if (obj is Type type)
            return ReferenceEquals(Type, type);
        return false;
    }

    public override int GetHashCode() => Type.GetHashCode();

    public static implicit operator TypeKey(Type value) => new TypeKey(value);

    public static implicit operator Type(TypeKey value) => value.Type;
}

public class TypeDictionaryBenchmarks { }
