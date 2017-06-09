﻿using System;
using System.Runtime.InteropServices;

namespace Bearded.TD.Utilities
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Id
    {
        private readonly int value;

        public int Value => value;

        public Id(int value)
        {
            this.value = value;
        }

        public Id<T> Generic<T>() => new Id<T>(value);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Id<T> : IEquatable<Id<T>>
    {
        private readonly int value;

        public int Value => value;
        
        public Id(int value)
        {
            this.value = value;
        }

        public Id Simple => new Id(value);

        public bool IsValid => value != 0;
        public static Id<T> Invalid => new Id<T>(0);

        public override int GetHashCode() => value.GetHashCode();
        public bool Equals(Id<T> other) => value.Equals(other.value);
        public override bool Equals(object obj) => obj is Id<T> && Equals((Id<T>)obj);
        public static bool operator ==(Id<T> left, Id<T> right) => left.Equals(right);
        public static bool operator !=(Id<T> left, Id<T> right) => !(left == right);
    }
}