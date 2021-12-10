using Bearded.Graphics;
using Bearded.UI.Navigation;

namespace Bearded.TD.UI;

abstract class UpdateableNavigationNode<T> : NavigationNode<T>, UIUpdater.IUpdatable
{
    public bool Deleted { get; private set; }

    protected override void Initialize(DependencyResolver dependencies, T parameters)
    {
        dependencies.Resolve<UIUpdater>().Add(this);
    }

    public override void Terminate()
    {
        Deleted = true;
    }

    public abstract void Update(UpdateEventArgs args);
}