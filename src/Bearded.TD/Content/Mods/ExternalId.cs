using System;
using System.Diagnostics.CodeAnalysis;

namespace Bearded.TD.Content.Mods
{
    readonly struct ExternalId<T> : IEquatable<ExternalId<T>>
    {
        private const string invalidString = "invalid";

        public static ExternalId<T> Invalid = new(null);

        [MemberNotNull(nameof(Value))]
        public bool IsValid => Value != null;

        public string? Value { get; }

        private ExternalId(string? value)
        {
            Value = value;
        }

        public static ExternalId<T> FromLiteral(string literal) => new(literal);

        public bool Equals(ExternalId<T> other) => Value == other.Value;

        public override bool Equals(object? obj) => obj is ExternalId<T> other && Equals(other);

        public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;

        public override string ToString() => Value ?? invalidString;

        public static bool operator ==(ExternalId<T> left, ExternalId<T> right) => left.Equals(right);

        public static bool operator !=(ExternalId<T> left, ExternalId<T> right) => !(left == right);
    }
}
