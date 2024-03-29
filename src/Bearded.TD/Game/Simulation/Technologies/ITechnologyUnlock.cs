namespace Bearded.TD.Game.Simulation.Technologies;

interface ITechnologyUnlock
{
    string Description { get; }

    void Apply(FactionTechnology factionTechnology);
}