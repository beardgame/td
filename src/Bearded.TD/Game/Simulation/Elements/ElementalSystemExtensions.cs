using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Elements;

static class ElementalSystemExtensions
{
    public enum ApplicationResult
    {
        Applied,
        EntityNotFound,
        RollFailed,
    }

    public static ApplicationResult TryApplyEffect<T>(this GameObject obj, T effect, double probability = 1)
        where T : IElementalEffect<T>
    {
        if (!obj.TryGetSingleComponent<IElementSystemEntity>(out var entity))
        {
            return ApplicationResult.EntityNotFound;
        }
        return entity.TryApplyEffect(effect.WithProbability(probability))
            ? ApplicationResult.Applied
            : ApplicationResult.RollFailed;
    }

    public static ElementalEffectAttempt<T> WithProbability<T>(this T effect, double probability)
        where T : IElementalEffect<T> => new(effect, probability);
}
