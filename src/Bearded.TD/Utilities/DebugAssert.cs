using System;
using System.Diagnostics;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global

namespace Bearded.TD.Utilities;

static class DebugAssert
{
    public static class Argument
    {
        [Conditional("DEBUG")]
        public static void IsNotNull<T>(T obj) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException();
        }

        [Conditional("DEBUG")]
        public static void Satisfies(bool condition)
        {
            if (!condition)
                throw new ArgumentException();
        }

        [Conditional("DEBUG")]
        public static void Satisfies(Func<bool> condition)
        {
            if (!condition())
                throw new ArgumentException();
        }

        [Conditional("DEBUG")]
        public static void Satisfies(bool condition, string message)
        {
            if (!condition)
                throw new ArgumentException(message);
        }

        [Conditional("DEBUG")]
        public static void Satisfies(Func<bool> condition, string message)
        {
            if (!condition())
                throw new ArgumentException(message);
        }

        [Conditional("DEBUG")]
        public static void IsFraction(float f) => Satisfies(f is >= 0 and <= 1);
    }

    public static class State
    {
        [Conditional("DEBUG")]
        public static void IsInvalid()
        {
            throw new InvalidOperationException();
        }

        [Conditional("DEBUG")]
        public static void IsInvalid(string message)
        {
            throw new InvalidOperationException(message);
        }

        [Conditional("DEBUG")]
        public static void Satisfies(bool condition)
        {
            if (!condition)
                throw new InvalidOperationException();
        }

        [Conditional("DEBUG")]
        public static void Satisfies(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        [Conditional("DEBUG")]
        public static void Satisfies(Func<bool> condition)
        {
            if (!condition())
                throw new ArgumentException();
        }

        [Conditional("DEBUG")]
        public static void Satisfies(Func<bool> condition, string message)
        {
            if (!condition())
                throw new ArgumentException(message);
        }
    }
}
