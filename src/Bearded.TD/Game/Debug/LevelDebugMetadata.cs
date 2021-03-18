using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.Graphics;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Debug
{
    sealed class LevelDebugMetadata
    {
        private readonly List<DebugLineSegment> segments = new List<DebugLineSegment>();

        public ReadOnlyCollection<DebugLineSegment> Segments { get; }

        public LevelDebugMetadata()
        {
            Segments = segments.AsReadOnly();
        }

        public void AddSegment(Position2 from, Position2 to, Color color)
        {
            segments.Add(new DebugLineSegment(from, to, color));
        }

        public readonly struct DebugLineSegment
        {
            public Position2 From { get; }
            public Position2 To { get; }
            public Color Color { get; }

            public DebugLineSegment(Position2 from, Position2 to, Color color)
            {
                From = from;
                To = to;
                Color = color;
            }
        }

        public void Clear()
        {
            segments.Clear();
        }
    }
}
