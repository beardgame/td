namespace Bearded.TD.Game.Simulation.Resources
{
    interface IResourceConsumer
    {
        void ConsumeResources(ResourceGrant grant);
    }
}
