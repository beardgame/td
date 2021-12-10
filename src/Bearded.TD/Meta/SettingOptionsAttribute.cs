using System;

namespace Bearded.TD.Meta;

class SettingOptionsAttribute : Attribute
{
    public object[] Options { get; }

    public SettingOptionsAttribute(params object[] options)
    {
        Options = options;
    }
}