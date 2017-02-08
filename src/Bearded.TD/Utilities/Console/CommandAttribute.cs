using System;

namespace Bearded.TD.Console
{
    [AttributeUsage(AttributeTargets.Method)]
    internal class CommandAttribute : Attribute
    {
        public CommandAttribute(string name, string parameterCompletion = null)
        {
            this.Name = name;
            this.ParameterCompletion = parameterCompletion;
        }

        public string Name { get; }
        public string ParameterCompletion { get; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class DebugCommandAttribute : CommandAttribute
    {
        public DebugCommandAttribute(string name, string parameterCompletion = null)
            : base(name, parameterCompletion)
        {
        }
    }
}
