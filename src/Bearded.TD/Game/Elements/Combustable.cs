using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Elements
{
    sealed class Combustable
    {
        public event VoidEventHandler Ignited;
        public event VoidEventHandler Extinguished;

        // How fast the fuel is consumed when it is on fire.
        public EnergyConsumptionRate BurningSpeed { get; }

        // When the fuel is hot enough it can catch on fire.
        public Energy FlashPointThreshold { get; }

        public Energy CurrentHeat { get; private set; }

        // How much fuel is left.
        public Energy Fuel { get; private set; }

        public bool IsOnFire { get; private set; }

        public bool IsDepleted => Fuel <= Energy.Zero;

        public Combustable(
            Energy initialFuel,
            Energy flashPointThreshold,
            EnergyConsumptionRate burningSpeed)
        {
            Fuel = initialFuel;
            FlashPointThreshold = flashPointThreshold;
            BurningSpeed = burningSpeed;
        }

        public void HitWithFire(Energy energy)
        {
            CurrentHeat += energy;
        }

        public void Update(TimeSpan elapsedTime)
        {
            if (!IsOnFire)
            {
                if (CurrentHeat < FlashPointThreshold) return;
                IsOnFire = true;
                Ignited?.Invoke();
            }

            Fuel -= elapsedTime * BurningSpeed;
            if (!IsDepleted) return;

            Fuel = Energy.Zero;
            IsOnFire = false;
            Extinguished?.Invoke();
        }
    }
}
