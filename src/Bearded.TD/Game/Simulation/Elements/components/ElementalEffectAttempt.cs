namespace Bearded.TD.Game.Simulation.Elements;

readonly record struct ElementalEffectAttempt<T>(T Effect, double Probability) where T : IElementalEffect<T>;
