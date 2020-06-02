using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Elements
{
    sealed class Combustable
    {
        public event VoidEventHandler? Ignited;
        public event VoidEventHandler? Extinguished;

        // How fast the fuel is consumed when it is on fire.
        public EnergyConsumptionRate BurningSpeed { get; }

        // When the fuel is hot enough it can catch on fire.
        public Energy FlashPointThreshold { get; }

        public Energy CurrentHeat { get; private set; }

        // How much fuel is left.
        public Energy Fuel { get; private set; }

        public bool IsOnFire { get; private set; }

        public bool IsDepleted => Fuel <= Energy.Zero;

        private bool isWetThisFrame;
        private bool hasSparkThisFrame;

        public Combustable(
            Energy initialFuel,
            Energy flashPointThreshold,
            EnergyConsumptionRate burningSpeed,
            bool isOnFire)
        {
            Fuel = initialFuel;
            FlashPointThreshold = flashPointThreshold;
            BurningSpeed = burningSpeed;
            IsOnFire = isOnFire;
        }

        public void Spark()
        {
            hasSparkThisFrame = true;
        }

        public void HitWithFire(Energy energy)
        {
            hasSparkThisFrame = true;

            CurrentHeat += energy;
        }

        public void HitWithWater(Energy energy)
        {
            isWetThisFrame = true;

            if (CurrentHeat > Energy.Zero)
            {
                CurrentHeat = SpaceTime1Math.Max(CurrentHeat - energy, Energy.Zero);
            }
        }

        public void Update(TimeSpan elapsedTime)
        {
            // State changes
            if (IsOnFire)
            {
                if (isWetThisFrame || IsDepleted)
                {
                    IsOnFire = false;
                    Extinguished?.Invoke();
                }
            }
            else
            {
                if (hasSparkThisFrame && !isWetThisFrame && CurrentHeat >= FlashPointThreshold)
                {
                    IsOnFire = true;
                    Ignited?.Invoke();
                }
            }

            // Fuel burning
            if (IsOnFire)
            {
                Fuel = SpaceTime1Math.Max(Fuel - elapsedTime * BurningSpeed, Energy.Zero);
            }

            // Reset for next frame
            isWetThisFrame = false;
            hasSparkThisFrame = false;
        }
    }
}
