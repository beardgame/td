namespace Bearded.UI.Navigation
{
    public interface INavigationNode
    {
        void Terminate();
    }

    public abstract class NavigationNode<T> : INavigationNode
    {
        protected NavigationController Navigation { get; private set; }

        internal void Initialize(NavigationContext<T> context)
        {
            Navigation = context.Navigation;
            Initialize(context.Dependencies, context.Parameters);
        }

        protected abstract void Initialize(DependencyResolver dependencies, T parameters);

        public virtual void Terminate() { }
    }
}
