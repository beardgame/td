using System;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Components
{
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(IComponent<>))]
    [MeansImplicitUse]
    class ComponentAttribute : Attribute
    {
        public string Id { get; }

        public ComponentAttribute(string id)
        {
            Id = id;
        }
    }
}
