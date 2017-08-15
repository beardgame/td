namespace Bearded.TD.Game.Generation.Enemies
{
    struct EnemySpawnDebugParameters
    {
        public static EnemySpawnDebugParameters Empty => new EnemySpawnDebugParameters();

        public double Debit { get; }
        public double PayoffFactor { get; }
        public double MinWaveCost { get; }
        public double MaxWaveCost { get; }
        public double Lag { get; }

        public EnemySpawnDebugParameters(
            double debit, double payoffFactor, double minWaveCost, double maxWaveCost, double lag)
        {
            Debit = debit;
            PayoffFactor = payoffFactor;
            MinWaveCost = minWaveCost;
            MaxWaveCost = maxWaveCost;
            Lag = lag;
        }
    }
}
