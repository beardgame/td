using Bearded.TD.Utilities.SpaceTime;
using static System.Math;
using static Bearded.TD.Constants.Game.Elements;
using static Bearded.TD.Utilities.SpaceTime.SpaceTime1Math;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Elements
{
    // TODO: do we deal with a certain amount of fuel?

    sealed class Temperature
    {
        public double Wetness { get; private set; }
        public Energy Heat { get; private set; }

        public bool IsFrozen => Heat <= FrozenThreshold;
        private bool instantlyEvaporatesWater => Heat >= InstantEvaporationThreshold;

        private Energy energyThisFrame;
        private double amountOfWaterThisFrame;

        // TODO: is there a different about fire and energy?
        // TODO: should there be a way to spark?
        public void HitWithEnergy(Energy energy)
        {
            energyThisFrame += energy;
        }

        public void HitWithWater(double amountOfWater)
        {
            amountOfWaterThisFrame += amountOfWater;
        }

        public void Simulate(TimeSpan elapsedTime)
        {
            Heat = Min(Heat + energyThisFrame, MaxHeat);

            if (instantlyEvaporatesWater)
            {
                simulateEvaporatedWater(amountOfWaterThisFrame);
            }
            else
            {
                Wetness = Min(Wetness + WaterToWetnessRatio * amountOfWaterThisFrame, MaxWetness);
            }

            evaporateWater(elapsedTime);

            // TODO: freeze
            // TODO: ignite

            resetForNextFrame();
        }

        private void resetForNextFrame()
        {
            energyThisFrame = Energy.Zero;
            amountOfWaterThisFrame = 0;
        }

        private void evaporateWater(TimeSpan elapsedTime)
        {
            if (Wetness <= MinWetness)
            {
                return;
            }

            // TODO: is evaporation temperature dependent?
            var maxPossibleEvaporation = (Wetness - MinWetness) / WaterEvaporatedPerSecond;
            var amountEvaporated = Min(maxPossibleEvaporation, elapsedTime.NumericValue * WaterEvaporatedPerSecond);
            Wetness -= amountEvaporated;
            simulateEvaporatedWater(amountEvaporated);
        }

        private void simulateEvaporatedWater(double amount)
        {
            var energyLoss = amount * EnergyPerUnitWaterEvaporated;
            Heat = Max(Heat - energyLoss, MinHeat);
        }
    }
}
