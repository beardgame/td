using Bearded.TD.Utilities.SpaceTime;
using static System.Math;
using static Bearded.TD.Constants.Game.Elements;
using static Bearded.TD.Utilities.SpaceTime.SpaceTime1Math;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Elements
{
    sealed class Temperature
    {
        public double Wetness { get; private set; }
        public Energy Heat { get; private set; }

        private Energy energyThisFrame;
        private double amountOfWaterThisFrame;

        public void AddEnergy(Energy energy)
        {
            energyThisFrame += energy;
        }

        public void AddWater(double amountOfWater)
        {
            amountOfWaterThisFrame += amountOfWater;
        }

        public void AdvanceSimulation(TimeSpan elapsedTime)
        {
            Heat += energyThisFrame;
            Wetness += amountOfWaterThisFrame;

            approachAmbientHeat(elapsedTime);
            evaporateWater(elapsedTime);
            resetForNextFrame();
        }

        private void resetForNextFrame()
        {
            energyThisFrame = Energy.Zero;
            amountOfWaterThisFrame = 0;
        }

        private void approachAmbientHeat(TimeSpan elapsedTime)
        {
            Heat = AmbientHeat + (Heat - AmbientHeat) * Pow(.5, elapsedTime / AmbientHeatApproachHalfTime);
        }

        private void evaporateWater(TimeSpan elapsedTime)
        {
            if (Wetness <= MinWetness)
            {
                Wetness = MinWetness;
                return;
            }

            // TODO: make half-life heat dependent.
            var newWetness = Wetness * Pow(.5, elapsedTime / WaterEvaporationHalfTime);
            var amountEvaporated = Wetness - newWetness;
            Wetness = newWetness;
            simulateEvaporatedWater(amountEvaporated);
        }

        private void simulateEvaporatedWater(double amount)
        {
            var energyLoss = amount * EnergyPerUnitWaterEvaporated;
            Heat = Max(Heat - energyLoss, MinHeat);
        }
    }
}
