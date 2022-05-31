using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Elements;

static class ElementalSystemExtensions
{
    public static bool TryApplyEffect<T>(this GameObject obj, T effect) where T : IElementalEffect
    {
        if (!obj.TryGetSingleComponent<IElementSystemEntity>(out var entity))
        {
            return false;
        }
        entity.ApplyEffect(effect);
        return true;
    }
}
