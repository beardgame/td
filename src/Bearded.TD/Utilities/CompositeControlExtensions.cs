using System.Collections.Generic;
using Bearded.UI.Controls;

// ReSharper disable once CheckNamespace
// The below extension method is not discovered automatically when used in collection initializers as intended.
namespace Bearded.TD;

static class CompositeControlExtensions
{
    public static void Add(this CompositeControl parent, IEnumerable<Control> children)
    {
        foreach (var child in children)
        {
            parent.Add(child);
        }
    }
}
