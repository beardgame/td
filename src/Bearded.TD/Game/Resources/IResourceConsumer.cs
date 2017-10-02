namespace Bearded.TD.Game.Resources
{
    interface IResourceConsumer
    {
        void ConsumeResources(ResourceGrant grant);
    }
}