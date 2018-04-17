using System;

namespace Bearded.TD.Game.Components
{
    [AttributeUsage(AttributeTargets.Class)]
    class ComponentAttribute : Attribute
    {
        public string Id { get; }

        public ComponentAttribute(string id)
        {
            Id = id;
        }
    }
}
