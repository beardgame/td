using System.Runtime.CompilerServices;
using Bearded.TD.Content.Components;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Rules;

namespace Bearded.TD.Tests.Content
{
    public static class ModLoadingInitializer
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            ComponentFactories.Initialize();
            FactionBehaviorFactories.Initialize();
            GameRuleFactories.Initialize();
            NodeBehaviorFactories.Initialize();
            TriggerFactories.Initialize();
        }
    }
}
