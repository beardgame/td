using System.Collections.Generic;
using System.Linq;

namespace Bearded.TD.Game;

sealed partial class ProtoLegs
{
    private sealed class LegGroup
    {
        public List<Leg> Legs { get; } = new ();

        public float GetError(ProtoLegs body)
        {
            return Legs.Sum(
                l =>
                {
                    var optimalLocation = Leg.OptimalLocationFrom(body, l.NeutralDirection);
                    var differenceToOptimalLocation = l.Position - optimalLocation;
                    return differenceToOptimalLocation.Length.NumericValue;
                });
        }

        public void StartStep(ProtoLegs body)
        {
            foreach (var leg in Legs)
            {
                leg.StartStep(body);
            }
        }

        public void EndStep()
        {
            foreach (var leg in Legs)
            {
                leg.EndStep();
            }
        }
    }
}
