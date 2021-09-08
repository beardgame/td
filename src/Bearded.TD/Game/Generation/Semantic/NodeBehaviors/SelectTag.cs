using System;
using System.Runtime.Serialization;
using Bearded.TD.Game.Generation.Semantic.Features;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Generation.Semantic.NodeBehaviors
{
    [NodeBehavior("selectTag")]
    sealed class SelectTag : NodeBehavior<SelectTag.BehaviorParameters>
    {
        private readonly Predicate<double> valuePassesThreshold;
        private readonly string tag;

        public record BehaviorParameters(double Threshold, string? Tag, CompareMode Mode);

        public enum CompareMode
        {
            [EnumMember(Value = ">")] Greater = 0,
            [EnumMember(Value = ">=")] GreaterOrEqual,
            [EnumMember(Value = "<")] Less,
            [EnumMember(Value = "<=")] LessOrEqual
        }

        public SelectTag(BehaviorParameters parameters) : base(parameters)
        {
            valuePassesThreshold = getValuePassesThresholdPredicate(parameters);
            tag = parameters.Tag ?? "default";
        }

        public override void Generate(NodeGenerationContext context)
        {
            context.Tiles.Selection.RemoveAll();

            foreach (var tile in context.Tiles.All)
            {
                if (tilePassesThreshold(tile, context))
                    context.Tiles.Selection.Add(tile);
            }
        }

        private bool tilePassesThreshold(Tile tile, NodeGenerationContext context)
        {
            var tagValueAtTile = context.Tiles.Tags[tag][tile];
            return valuePassesThreshold(tagValueAtTile);
        }

        private static Predicate<double> getValuePassesThresholdPredicate(BehaviorParameters parameters)
        {
            return parameters.Mode switch
            {
                CompareMode.Greater => value => value > parameters.Threshold,
                CompareMode.GreaterOrEqual => value => value >= parameters.Threshold,
                CompareMode.Less => value => value < parameters.Threshold,
                CompareMode.LessOrEqual => value => value <= parameters.Threshold,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
