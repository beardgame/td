namespace Bearded.TD.Game.Simulation.Footprints;

interface ITilePresenceListener
{
    /// <summary>
    /// Detach will first send leave events for all current tiles before stopping to listen to tile change events.
    /// </summary>
    void Detach();

    /// <summary>
    /// Stop listening immediately will immediately stop all events, but not send leave events for existing tiles.
    /// </summary>
    void StopListeningImmediately();
}
