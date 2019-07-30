using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Utilities.SpaceTime
{
    struct FlowRate
    {
        private readonly float value;

        public float NumericValue => value;

        public FlowRate(float value)
        {
            this.value = value;
        }

        public static Volume operator *(FlowRate flowRate, TimeSpan timeSpan)
        {
            return new Volume((float)(flowRate.value * timeSpan.NumericValue));
        }
    }
}