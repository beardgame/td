using Bearded.TD.Utilities.Input;

namespace Bearded.TD.Game.UI
{
    struct ActionState
    {
        public bool Hit { get; }
        public bool Active { get; }
        public bool Released { get; }

        public bool IsAnalog { get; }
        public float AnalogAmount { get; }

        public ActionState(IAction action)
        {
            Hit = action.Hit;
            Active = action.Active;
            Released = action.Released;
            IsAnalog = action.IsAnalog;
            AnalogAmount = action.AnalogAmount;
        }
    }
}
