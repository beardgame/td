using System.Collections.Immutable;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Testing.Components;

sealed class GameObjectBlueprintFactory
{
    public static IGameObjectBlueprint Empty() => new BlueprintImplementation([]);

    public static IGameObjectBlueprint WithPremadeComponents(params IComponent[] components) =>
        new BlueprintImplementation(components);

    private sealed class BlueprintImplementation(IEnumerable<IComponent> components) : IGameObjectBlueprint
    {
        private readonly ImmutableArray<IComponent> components = [..components];

        public ModAwareId Id => ModAwareId.Invalid;
        public IEnumerable<IComponent> GetComponents() => components;
    }
}
