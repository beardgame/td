namespace Bearded.TD.Game.Simulation.Damage
{
    interface IHealthReport
    {
        public HitPoints CurrentHitPoints { get; }
        public HitPoints MaxHitPoints { get; }
    }
}
