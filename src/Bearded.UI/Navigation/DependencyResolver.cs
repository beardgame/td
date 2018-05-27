using System;
using System.Collections.Generic;

namespace Bearded.UI.Navigation
{
    public sealed class DependencyResolver
    {
        private readonly Dictionary<Type, object> dict = new Dictionary<Type, object>();

        public void Add<T>(T dependency)
        {
            dict[typeof(T)] = dependency;
        }

        public T Resolve<T>()
        {
            return (T) dict[typeof(T)];
        }

        public bool TryResolve<T>(out T value)
        {
            if (dict.TryGetValue(typeof(T), out var obj))
            {
                value = (T) obj;
                return true;
            }

            value = default(T);
            return false;
        }
    }
}
