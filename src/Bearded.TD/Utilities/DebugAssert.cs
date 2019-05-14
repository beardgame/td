using System;
using System.Diagnostics;
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global

namespace Bearded.TD.Utilities
{
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
            public static void Satisfies(bool condition, string message)
            {
                if (!condition)
                    throw new ArgumentException(message);
            }
        }

        public static class State
        {
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
        }
    }
}
