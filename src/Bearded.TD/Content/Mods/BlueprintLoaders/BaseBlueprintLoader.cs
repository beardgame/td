using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Serialization.Converters;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game;
using Bearded.TD.Utilities;
using Newtonsoft.Json;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Content.Mods.BlueprintLoaders
{
    abstract class BaseBlueprintLoader<TBlueprint, TJsonModel, TResolvers>
        where TBlueprint : IBlueprint
        where TJsonModel : IConvertsTo<TBlueprint, TResolvers>
    {
        protected delegate ReadonlyBlueprintCollection<TBlueprint> DependencySelector(Mod mod);

        protected BlueprintLoadingContext Context { get; }

        protected abstract string RelativePath { get; }

        protected virtual DependencySelector SelectDependency { get; }

        protected BaseBlueprintLoader(BlueprintLoadingContext context)
        {
            Context = context;
        }

        public virtual ReadonlyBlueprintCollection<TBlueprint> LoadBlueprints()
        {
            var files = GetJsonFiles();

            var blueprints = LoadBlueprintsFromFiles(files);

            var blueprintCollection = new ReadonlyBlueprintCollection<TBlueprint>(blueprints);

            SetupDependencyResolver(blueprintCollection);

            return blueprintCollection;
        }

        protected virtual void SetupDependencyResolver(ReadonlyBlueprintCollection<TBlueprint> blueprintCollection)
        {
            var selector = SelectDependency;

            if (selector == null)
                return;

            var dependencyResolver = new BlueprintDependencyResolver<TBlueprint>(
                Context.Meta, blueprintCollection, Enumerable.Empty<Mod>(), m => selector(m));

            Context.Serializer.Converters.Add(new DependencyConverter<TBlueprint>(dependencyResolver));
        }

        protected virtual FileInfo[] GetJsonFiles()
        {
            var totalPath = Path.Combine(Context.Meta.Directory.FullName, RelativePath);

            if (!Directory.Exists(totalPath))
                return new FileInfo[0];

            return Context.Meta
                .Directory
                .GetDirectories(RelativePath, SearchOption.TopDirectoryOnly)
                .SingleOrDefault()
                ?.GetFiles("*.json", SearchOption.AllDirectories)
                ?? new FileInfo[0];
        }

        protected virtual List<TBlueprint> LoadBlueprintsFromFiles(FileInfo[] files)
        {
            var blueprints = new List<TBlueprint>();

            foreach (var file in files)
            {
                try
                {
                    var blueprint = LoadBlueprint(file);

                    blueprints.Add(blueprint);
                }
                catch (Exception e)
                {
                    LogException(e, $"Error loading '{Context.Meta.Id}/{RelativePath}/../{file.Name}': {e.Message}");
                }
            }

            return blueprints;
        }

        protected virtual TBlueprint LoadBlueprint(FileInfo file)
        {
            var text = file.OpenText();
            var reader = new JsonTextReader(text);
            var jsonBlueprint = Context.Serializer.Deserialize<TJsonModel>(reader);
            var dependencyResolvers = GetDependencyResolvers(file);
            var blueprint = jsonBlueprint.ToGameModel(dependencyResolvers);

            if (Path.GetFileNameWithoutExtension(file.FullName) != blueprint.Id)
            {
                LogWarning($"Loaded blueprint {Context.Meta.Id}.{blueprint.Id} with mismatching filename {file.Name}");
            }

            return blueprint;
        }

        protected virtual TResolvers GetDependencyResolvers(FileInfo file)
        {
            DebugAssert.State.Satisfies(typeof(TResolvers) == typeof(Void), "Override GetDependencyResolver!");

            return default(TResolvers);
        }

        protected void LogException(Exception exception, string customMessage)
        {
            LogError(customMessage);
            LogDebug(exception.StackTrace);
        }

        protected void LogError(string message)
        {
            Context.Context.Logger.Error?.Log(message);
        }

        protected void LogWarning(string message)
        {
            Context.Context.Logger.Warning?.Log(message);
        }

        protected void LogDebug(string message)
        {
            Context.Context.Logger.Debug?.Log(message);
        }
    }
}
