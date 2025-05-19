using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Elements;

readonly record struct PreviewElementalEffectAttempt<T>(ElementalEffectAttempt<T> Attempt) : IComponentPreviewEvent
    where T : IElementalEffect<T>
{
    public PreviewElementalEffectAttempt<T> WithProbability(double probability) =>
        new(Attempt with { Probability = probability });

    public PreviewElementalEffectAttempt<T> WithEffect(T effect) =>
        new(Attempt with { Effect = effect });
}
