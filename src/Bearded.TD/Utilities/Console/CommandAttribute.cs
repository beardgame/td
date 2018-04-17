using System;

namespace Bearded.TD.Utilities.Console
{
    [AttributeUsage(AttributeTargets.Method)]
    class CommandAttribute : Attribute
    {
        public string Name { get; }
        public string ParameterCompletion { get; }

        public CommandAttribute(string name, string parameterCompletion = null)
        {
            Name = name;
            ParameterCompletion = parameterCompletion;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    class DebugCommandAttribute : CommandAttribute
    {
        public DebugCommandAttribute(string name, string parameterCompletion = null)
            : base(name, parameterCompletion)
        {
        }
    }
}
