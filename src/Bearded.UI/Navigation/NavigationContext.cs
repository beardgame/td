namespace Bearded.UI.Navigation
{
    sealed class NavigationContext<T>
    {
        public NavigationController Navigation { get; }
        public DependencyResolver Dependencies { get; }
        public T Parameters { get; }
        
        public NavigationContext
            (NavigationController navigationController, DependencyResolver dependencies, T parameters)
        {
            Navigation = navigationController;
            Dependencies = dependencies;
            Parameters = parameters;
        }
    }
}
