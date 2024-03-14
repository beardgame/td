using Bearded.Graphics;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities;

sealed class TimeSource : ITimeSource
{
    public Instant Time { get; private set; }

    public void Update(UpdateEventArgs args)
    {
        Time += args.ElapsedTimeInS.S();
    }

    public void SetTo(Instant time)
    {
        Time = time;
    }
}
