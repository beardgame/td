using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.GameLoop;

sealed partial class EnemyFormGenerator
{
    public bool CanGenerate(IGameObjectBlueprint blueprint, Requirements requirements)
    {
        var summary = findSummary(blueprint);

        return summary.SupportedElements.Contains(requirements.AffinityElement);
    }
}
