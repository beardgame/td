namespace Bearded.TD.Game.GameState.Resources
{
    interface IResourceConsumer
    {
        void ConsumeResources(ResourceGrant grant);
    }
}
