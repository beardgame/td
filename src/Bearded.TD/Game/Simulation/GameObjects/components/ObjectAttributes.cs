using Bearded.TD.Content.Mods;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameObjects;

[Component("attributes")]
sealed class ObjectAttributes(ObjectAttributes.IParameters parameters)
    : Component<ObjectAttributes.IParameters>(parameters), IObjectAttributes
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        public string Name { get; }
        public string? Description { get; }
        public ModAwareSpriteId? Icon { get; }
    }

    public string Name => Parameters.Name;
    public string? Description => Parameters.Description;
    public ModAwareSpriteId? Icon => Parameters.Icon;

    protected override void OnAdded() { }
    public override void Update(TimeSpan elapsedTime) { }

    public static ObjectAttributes Default { get; } =
        new(new ObjectAttributesParametersTemplate("Unnamed object", null, null));
}

interface IObjectAttributes
{
    string Name { get; }
    string? Description { get; }
    ModAwareSpriteId? Icon { get; }
}

static class ObjectAttributesExtensions
{
    public static string NameOrDefault(this IGameObjectBlueprint blueprint) => blueprint.AttributesOrDefault().Name;

    public static string NameOrDefault(this GameObject obj) => obj.AttributesOrDefault().Name;

    public static IObjectAttributes AttributesOrDefault(this IGameObjectBlueprint blueprint) =>
        GameObjectFactory.CreateFromBlueprintWithoutRenderer(blueprint, null, Position3.Zero).AttributesOrDefault();

    public static IObjectAttributes AttributesOrDefault(this GameObject obj) =>
        obj.TryGetSingleComponent<IObjectAttributes>(out var attributes) ? attributes : ObjectAttributes.Default;
}
