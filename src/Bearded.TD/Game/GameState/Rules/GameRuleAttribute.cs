using System;
using Bearded.TD.Content.Behaviors;
using JetBrains.Annotations;

namespace Bearded.TD.Game.GameState.Rules
{
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(IGameRule<>))]
    [MeansImplicitUse]
    sealed class GameRuleAttribute : Attribute, IBehaviorAttribute
    {
        public string Id { get; }

        public GameRuleAttribute(string id)
        {
            Id = id;
        }
    }
}
