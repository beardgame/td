using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Bearded.TD.Audio;
using Bearded.TD.Content.Components;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods.BlueprintLoaders;
using Bearded.TD.Content.Serialization.Converters;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Drawing.Animation;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using Newtonsoft.Json;
using FactionBlueprint = Bearded.TD.Content.Serialization.Models.FactionBlueprint;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Content.Mods;

static class ModLoader
{
    public static async Task<Mod> Load(ModLoadingContext context, ModMetadata mod, ReadOnlyCollection<Mod> loadedDependencies)
    {
        return await new Loader(context, mod, loadedDependencies).Load();
    }

    private sealed class Loader(
        ModLoadingContext context,
        ModMetadata meta,
        ReadOnlyCollection<Mod> loadedDependencies)
    {
        public async Task<Mod> Load()
        {
            return await Task.Run(load);
        }

        private Mod load()
        {
            context.Profiler.StartLoading();

            var serializer = configureSerializer();

            var loadingContext = new BlueprintLoadingContext(context, meta, serializer, loadedDependencies);

            var tags = new UpgradeTagResolver(meta, loadedDependencies);

            var shaders = new ShaderBlueprintLoader(loadingContext).LoadBlueprints();
            var materials = new MaterialBlueprintLoader(loadingContext).LoadBlueprints();
            var sprites = new SpriteBlueprintLoader(loadingContext).LoadBlueprints();
            var models = new ModelBlueprintLoader(loadingContext).LoadBlueprints();
            var fontDefinitions = new FontDefinitionBlueprintLoader(loadingContext).LoadBlueprints();
            var fonts = new FontBlueprintLoader(loadingContext).LoadBlueprints();
            var soundEffects = new SoundBlueprintLoader(loadingContext).LoadBlueprints();
            var footprints = new FootprintBlueprintLoader(loadingContext).LoadBlueprints();
            var gameObjects = new GameObjectBlueprintLoader(loadingContext).LoadBlueprints();
            var upgrades = new UpgradeBlueprintLoader(loadingContext).LoadBlueprints();
            var modules = new ModuleBlueprintLoader(loadingContext).LoadBlueprints();
            var technologies = new TechnologyBlueprintLoader(loadingContext, gameObjects, upgrades).LoadBlueprints();
            var levelNodes = new NodeBlueprintLoader(loadingContext).LoadBlueprints();
            var biomes = new BiomeLoader(loadingContext).LoadBlueprints();
            var gameModes = new GameModeBlueprintLoader(loadingContext).LoadBlueprints();

            context.Profiler.FinishLoading();
            context.Logger.Debug?.Log(
                $"Mod {meta.Id} finished loading in {context.Profiler.TotalElapsedTime:s\\.fff}s");

            var blueprints = new Blueprints(
                shaders,
                materials,
                sprites,
                models,
                fontDefinitions,
                fonts,
                soundEffects,
                footprints,
                gameObjects,
                upgrades,
                modules,
                technologies,
                levelNodes,
                biomes,
                gameModes);

            return new Mod(meta.Id, meta.Name, blueprints, tags.GetForCurrentMod());
        }

        private JsonSerializer configureSerializer()
        {
            var serializer = new JsonSerializer();
            serializer.Converters.AddRange(
                new StepConverter(),
                new TileConverter(),
                new SpaceTime1Converter<Unit>(v => v.U()),
                new SpaceTime1Converter<Speed>(SpaceTimeExtensions.UnitsPerSecond),
                new SpaceTime1Converter<TimeSpan>(v => ((double) v).S()),
                new SpaceTime1Converter<Frequency>(v => ((double) v).PerSecond()),
                new SpaceTime1Converter<Direction2>(Direction2.FromDegrees),
                new SpaceTime1Converter<Acceleration>(a => new Acceleration(a)),
                new SpaceTime1Converter<Angle>(Angle.FromDegrees),
                new SpaceTime1Converter<AngularAcceleration>(AngularAcceleration.FromDegrees),
                new SpaceTime1Converter<Volume>(v => new Volume(v)),
                new SpaceTime1Converter<FlowRate>(r => new FlowRate(r)),
                new SpaceTime1Converter<AngularVelocity>(AngularVelocity.FromDegrees),
                new SpaceTime1Converter<ElectricCharge>(d => new ElectricCharge(d)),
                new SpaceTime1Converter<ElectricChargeRate>(d => new ElectricChargeRate(d)),
                new SpaceTime1Converter<HitPoints>(d => d.HitPoints()),
                new SpaceTime1Converter<Resource<Scrap>>(d => new Resource<Scrap>(d)),
                new SpaceTime1Converter<ResourcePerSecond<Scrap>>(d => new ResourcePerSecond<Scrap>(d)),
                new SpaceTime1Converter<Resource<CoreEnergy>>(d => new Resource<CoreEnergy>(d)),
                new SpaceTime1Converter<ResourcePerSecond<CoreEnergy>>(d => new ResourcePerSecond<CoreEnergy>(d)),
                new SpaceTime2Converter<Difference2>((x, y) => new Difference2(x, y)),
                new SpaceTime3Converter<Difference3>((x, y, z) => new Difference3(x, y, z)),
                new SpaceTime1Converter<Resistance>(f => new Resistance(f)),
                new SpaceTime1Converter<UntypedDamage>(d => new UntypedDamage(d.HitPoints())),
                new SpaceTime1Converter<UntypedDamagePerSecond>(d => new UntypedDamagePerSecond(d.HitPoints())),
                new SpaceTime2Converter<PitchRange>((from, to) => new PitchRange(from, to)),
                new SpaceTime3Converter<Velocity3>((x, y, z) => new Velocity3(x, y, z)),
                new SpaceTime1Converter<TemperatureDifference>(d => new TemperatureDifference(d)),
                new ColorConverter(),
                BehaviorConverterFactory.ForBaseComponents(),
                BehaviorConverterFactory.ForFactionBehaviors(),
                BehaviorConverterFactory.ForGameRules(),
                BehaviorConverterFactory.ForNodeBehaviors(),
                new ComponentFactoryConverter(),
                new FlattenedBlueprintConverter<IFactionBlueprint, FactionBlueprint>(meta),
                ModAwareIdConverter.WithinMod(meta),
                ModAwareSpriteIdConverter.WithinMod(meta),
                new ExternalIdConverter<Faction>(),
                new NodeTagConverter(),
                new SocketShapeConverter(),
                new TargetingModeConverter(),
                new TriggerConverter(),
                new UpgradeEffectConverter(),
                new UpgradePrerequisitesConverter(),
                GenericInterfaceConverter.From(typeof(IKeyFrameAnimation<>), typeof(KeyFrameAnimation<>))
            );
            foreach (var (key, value) in ParametersTemplateLibrary.TemplateTypeByInterface)
                serializer.Converters.Add(new InterfaceConverter(key, value));

            return serializer;
        }
    }
}
