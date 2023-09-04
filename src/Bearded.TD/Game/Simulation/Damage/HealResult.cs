namespace Bearded.TD.Game.Simulation.Damage;

readonly struct HealResult
{
    public HealInfo Heal { get; }

    public HealResult(HealInfo heal)
    {
        Heal = heal;
    }
}
