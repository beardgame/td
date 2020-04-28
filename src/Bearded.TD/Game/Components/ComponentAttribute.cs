using System;
using Bearded.TD.Content.Behaviors;
using Bearded.TD.Content.Serialization.Models;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Components
{
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(IComponent<>))]
    [MeansImplicitUse]
    class ComponentAttribute : Attribute, IBehaviorAttribute
    {
        public string Id { get; }

        public ComponentAttribute(string id)
        {
            Id = id;
        }
    }
}
