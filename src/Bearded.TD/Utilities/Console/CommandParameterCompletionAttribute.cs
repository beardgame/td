using System;

namespace Bearded.TD.Utilities.Console
{
    [AttributeUsage(AttributeTargets.Method)]
    class CommandParameterCompletionAttribute : Attribute
    {
        public string Name { get; }

        public CommandParameterCompletionAttribute(string name)
        {
            Name = name;
        }
    }
}
