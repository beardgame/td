using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation;
using Bearded.TD.Utilities;
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

        protected virtual DependencySelector? SelectDependency { get; } = null;

        protected virtual JsonSerializerOptions SerializerOptions => Context.SerializerOptions;

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

            Context.AddDependencyResolver(new BlueprintDependencyResolver<TBlueprint>(
                Context.Meta, blueprintCollection, Context.LoadedDependencies, m => selector(m)));
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
                finally
                {
                    Context.Context.Profiler.CleanUpLoadingBlueprint(path(file));
                }
            }

            return blueprints;
        }

        protected virtual TBlueprint LoadBlueprint(FileInfo file)
        {
            var filePath = path(file);
            Context.Context.Profiler.StartLoadingBlueprint(filePath);
            var hasWarnings = false;

            var jsonBlueprint = ParseJsonModel(file);
            var dependencyResolvers = GetDependencyResolvers(file);
            var blueprint = jsonBlueprint.ToGameModel(Context.Meta, dependencyResolvers);

            if (Path.GetFileNameWithoutExtension(file.FullName) != blueprint.Id.Id)
            {
                LogWarning($"Loaded blueprint {Context.Meta.Id}.{blueprint.Id} with mismatching filename {file.Name}");
                hasWarnings = true;
            }

            if (hasWarnings)
            {
                Context.Context.Profiler.FinishLoadingBlueprintWithWarnings(filePath);
            }
            else
            {
                Context.Context.Profiler.FinishLoadingBlueprintSuccessfully(filePath);
            }

            return blueprint;
        }

        private string path(FileSystemInfo file) => $"{Context.Meta.Id}/{RelativePath}/../{file.Name}";

        protected TJsonModel ParseJsonModel(FileInfo file)
        {
            var text = file.OpenText().ReadToEnd();
            return JsonSerializer.Deserialize<TJsonModel>(text, Context.SerializerOptions);
        }

        protected virtual TResolvers GetDependencyResolvers(FileInfo file)
        {
            DebugAssert.State.Satisfies(typeof(TResolvers) == typeof(Void), "Override GetDependencyResolver!");

            return default;
        }

        protected void LogException(Exception exception, string customMessage)
        {
            LogError(customMessage);
            if (exception.StackTrace != null)
            {
                LogDebug(exception.StackTrace);
            }
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
