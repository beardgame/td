# UI Factories
This namespace contains a set of factories that can be use to create UI controls and layouts. Its goals are:

* To standardise all usages of a control used across the entire project;
* To standardise layouts across the entire project;
* To provide an easy-to-use and consistent way to quickly create new and iterate on existing UI elements.

## Overall pattern
The overall pattern for a factory class is as follows:

```csharp
static class Factories
{
    // Optional direct method for common use cases.
    public static MyControl MyControl(/* parameters */) =>
        MyControl(builder =>
            {
                // Apply parameters to builder func.
                return builder;
            });

    public static MyControl MyControl(BuilderFunc<Builder> builderFunc)
    {
        var builder = new Builder();
        builderFunc(builder);
        return builder.Build();
    }

    public sealed class Builder
    {
        // Builder methods for the control.

        public MyControl Build()
        {
            // Create instance
        }
    }
}
```

Some notes and comments:

* The use of a builder makes it possible to construct different controls in an expressive way, without having to create a large number of overloads for the factory methods.
* We pass in a special `BuilderFunc` delegate that takes a builder instead of making a builder directly because it allows us to use a consistent pattern for composite controls. For example, a tab bar may allow adding buttons taking the same builder function, where the tab builder then ask some behaviour on top.
* The `BuilderFunc` delegate expects a builder to be returned. This makes it less likely that the caller calls the `Build` function themselves. Sadly it isn't possible to make the `Build` function only visible to the factory itself without significant more boilerplate.

## Layouts
On top of building individual controls, controls also need to be arranged in consistent ways. Layouts can help with that. Layouts vary slightly from builders in that they apply directly to a control. You pass in the control on which the layout should be built, and it will add the children to the control directly, making sure that the correct anchors are set.

`Layout` is the most generic layout, but specialised variants (such as the `ColumnLayout`) can be found in the codebase as well.

Note: `Layout` specifically has a `PristineLayout` version as well. This is a superclass of `Layout` that can be used to set some attributes that apply to the entire layout. As soon as the first child is added, these attributes are considered fixed.

## Composite controls
A lot of controls consist of several other controls combined in a consistent way.

### Extensions
To avoid a certain composite knowing about all the possible little bits it can add, extensions should be used liberally. For example, the `FormBuilder` doesn't actually know anything about all the controls it could contain. Other factories can add extensions on the `FormBuilder` and use the more generic methods on `FormBuilder` to provide to desired behaviour.

Note that extensions can generally only be used on composites that have an unbounded size (e.g. there is a list of children). Components that have components in fixed spots (e.g. the nav bar, having only a forward and backward button) may want to choose to provide the methods explicitly.

### Anchoring
In Bearded.UI, the parent is responsible for positioning a control. In other words, a type of control cannot enforce that it is always shown at a consistent size. Parents must thus make sure that they use the correct constants for sizing the controls. For this reason, it is desirable that the top level screen _only_ use layouts and composite controls, and all sizing should be done using global UI constants only.
