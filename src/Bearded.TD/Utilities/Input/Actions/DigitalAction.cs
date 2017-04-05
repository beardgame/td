
namespace Bearded.TD.Utilities.Input.Actions
{
    abstract class DigitalAction : IAction
    {
        public abstract bool Hit { get; }
        public abstract bool Active { get; }
        public abstract bool Released { get; }

        public bool IsAnalog => false;
        public float AnalogAmount => Active ? 1 : 0;

        public bool Equals(IAction other) => this.IsSameAs(other);
        public override int GetHashCode() => ToString().GetHashCode();
    }
}
