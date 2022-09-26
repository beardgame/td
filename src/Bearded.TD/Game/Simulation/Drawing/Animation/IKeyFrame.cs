using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Drawing.Animation;

interface IKeyFrame<T> where T : IKeyFrame<T>
{
    TimeSpan Duration { get; }
    T InterpolateTowards(T other, double t);
}
