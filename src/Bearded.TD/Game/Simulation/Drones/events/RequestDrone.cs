using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Drones;

readonly record struct RequestDrone(Faction Faction, DroneRequest Request, DroneFulfillmentPreview? FulfillmentPreview)
    : IGlobalPreviewEvent
{
    public RequestDrone OfferAlternative(DroneFulfillmentPreview preview)
    {
        if (FulfillmentPreview is not { } existingPreview || preview.Path.Cost < existingPreview.Path.Cost)
        {
            return this with { FulfillmentPreview = preview };
        }

        return this;
    }
}
