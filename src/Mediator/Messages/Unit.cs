namespace Mediator
{
    public readonly struct Unit : IEquatable<Unit>, IComparable<Unit>, IComparable
    {
        private static readonly Unit _value = new();

        public static ref readonly Unit Value => ref _value;

        public static ValueTask<Unit> ValueTask => new ValueTask<Unit>(_value);

        public int CompareTo(Unit other) => 0;

        int IComparable.CompareTo(object? obj) => 0;

        public override int GetHashCode() => 0;

        public bool Equals(Unit other) => true;

        public override bool Equals(object? obj) => obj is Unit;

        public static bool operator ==(Unit _, Unit __) => true;

        public static bool operator !=(Unit _, Unit __) => false;

        public override string ToString() => "()";
    }
}
