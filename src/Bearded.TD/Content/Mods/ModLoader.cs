using System.Linq;
using System.Threading.Tasks;
using amulware.Graphics.Serialization.JsonNet;
using Bearded.TD.Content.Mods.BlueprintLoaders;
using Bearded.TD.Content.Serialization.Converters;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Technologies;
using Bearded.TD.Game.Units;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using Newtonsoft.Json;
using BuildingBlueprintJson = Bearded.TD.Content.Serialization.Models.BuildingBlueprint;
using SpriteSetJson = Bearded.TD.Content.Serialization.Models.SpriteSet;
using ShaderJson = Bearded.TD.Content.Serialization.Models.Shader;
using TechnologyBlueprintJson = Bearded.TD.Content.Serialization.Models.TechnologyBlueprint;
using UpgradeBlueprintJson = Bearded.TD.Content.Serialization.Models.UpgradeBlueprint;

namespace Bearded.TD.Content.Mods
{
    static class ModLoader
    {
        public static async Task<Mod> Load(ModLoadingContext context, ModMetadata mod)
        {
            return await new Loader(context, mod).Load();
        }

        private sealed class Loader
        {
            private readonly ModLoadingContext context;
            private readonly ModMetadata meta;
            private JsonSerializer serializer;

            public Loader(ModLoadingContext context, ModMetadata meta)
            {
                this.context = context;
                this.meta = meta;
            }

            public async Task<Mod> Load()
            {
                return await Task.Run(() => load());
            }

            private Mod load()
            {
                configureSerializer();

                var loadingContext = new BlueprintLoadingContext(context, meta, serializer);

                var tags = new UpgradeTagResolver(meta, Enumerable.Empty<Mod>());

                var shaders = new ShaderBlueprintLoader(loadingContext).LoadBlueprints();
                var materials = new MaterialBlueprintLoader(loadingContext).LoadBlueprints();
                var sprites = new SpriteBlueprintLoader(loadingContext).LoadBlueprints();
                var componentOwners = new ComponentOwnerBlueprintLoader(loadingContext).LoadBlueprints();
                var footprints = new FootprintGroupBlueprintLoader(loadingContext).LoadBlueprints();
                var buildings = new BuildingBlueprintLoader(loadingContext, tags).LoadBlueprints();
                var units = new UnitBlueprintLoader(loadingContext).LoadBlueprints();
                var upgrades = new UpgradeBlueprintLoader(loadingContext).LoadBlueprints();
                var technologies = new TechnologyBlueprintLoader(loadingContext).LoadBlueprints();

                return new Mod(
                    meta.Id,
                    meta.Name,
                    shaders,
                    materials,
                    sprites,
                    footprints,
                    buildings,
                    units,
                    componentOwners,
                    upgrades,
                    technologies,
                    tags.GetForCurrentMod());
            }

            private void configureSerializer()
            {
                serializer = new JsonSerializer();
                serializer.Converters.AddRange(
                    new StepConverter(),
                    new SpaceTime1Converter<Unit>(v => v.U()),
                    new SpaceTime1Converter<Speed>(SpaceTimeExtensions.UnitsPerSecond),
                    new SpaceTime1Converter<TimeSpan>(v => ((double) v).S()),
                    new SpaceTime1Converter<Frequency>(v => ((double) v).PerSecond()),
                    new SpaceTime1Converter<Direction2>(Direction2.FromDegrees),
                    new SpaceTime1Converter<Angle>(Angle.FromDegrees),
                    new SpaceTime1Converter<AngularAcceleration>(AngularAcceleration.FromDegrees),
                    new SpaceTime1Converter<Volume>(v => new Volume(v)),
                    new SpaceTime1Converter<FlowRate>(r => new FlowRate(r)),
                    new SpaceTime1Converter<AngularAcceleration>(AngularAcceleration.FromDegrees),
                    new SpaceTime1Converter<Energy>(d => new Energy(d)),
                    new SpaceTime1Converter<EnergyConsumptionRate>(d => new EnergyConsumptionRate(d)),
                    new SpaceTime2Converter<Difference2>((x, y) => new Difference2(x, y)),
                    Converters.ColorContainerConverter,
                    ComponentConverterFactory.ForBuildingComponents(),
                    ComponentConverterFactory.ForBaseComponent(),
                    new TechnologyUnlockConverter(),
                    new UpgradeEffectConverter()
                );
                foreach (var (key, value) in ParametersTemplateLibrary.Instance.GetInterfaceToTemplateMap())
                    serializer.Converters.Add(new ComponentParameterTemplateConverter(key, value));
            }
        }
    }
}
