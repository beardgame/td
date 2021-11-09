namespace Bearded.TD.Game.Simulation.Components
{
    interface INestedComponentOwner
    {
        IComponentOwner NestedComponentOwner { get; }
    }
}
