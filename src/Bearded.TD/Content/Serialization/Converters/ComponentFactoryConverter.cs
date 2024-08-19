using System.IO;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.GameObjects;
using Newtonsoft.Json;
using IComponent = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Content.Serialization.Converters;

sealed class ComponentFactoryConverter : JsonConverterBase<IComponentFactory>
{
    protected override IComponentFactory ReadJson(JsonReader reader, JsonSerializer serializer)
    {
        var componentParameters = serializer.Deserialize<IComponent>(reader) ?? throw new InvalidDataException();
        return ComponentFactories.CreateComponentFactory(componentParameters);
    }
}
