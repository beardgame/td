using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities;

interface ITimeSource
{
    Instant Time { get; }
}