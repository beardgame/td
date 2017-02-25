namespace Bearded.TD.Game.Resources
{
    interface IResourceConsumer
    {
        double RatePerS { get; }
        double Maximum { get; }
        void ConsumeResources(ResourceGrant grant);
    }
}