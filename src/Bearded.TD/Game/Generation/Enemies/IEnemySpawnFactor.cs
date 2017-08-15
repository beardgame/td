namespace Bearded.TD.Game.Generation.Enemies
{
    interface IEnemySpawnFactor
    {
        void Apply(ref double probability, GameInstance game);
    }
}
