using System.Runtime.InteropServices;

// ReSharper disable ConvertToAutoProperty

namespace Bearded.TD.Game.Units
{
    [StructLayout(LayoutKind.Sequential)]
    struct EnemyUnitState
    {
        private readonly float x;
        private readonly float y;
        private readonly int goalTileX;
        private readonly int goalTileY;
        private readonly int health;
        private readonly int damage;
        private readonly double timeBetweenAttacks;
        private readonly float speed;

        public float X => x;
        public float Y => y;
        public int GoalTileX => goalTileX;
        public int GoalTileY => goalTileY;
        public int Health => health;
        public int Damage => damage;
        public double TimeBetweenAttacks => timeBetweenAttacks;
        public float Speed => speed;

        public EnemyUnitState(
                float x, float y, int goalTileX, int goalTileY,
                int health, int damage, double timeBetweenAttacks, float speed)
        {
            this.x = x;
            this.y = y;
            this.goalTileX = goalTileX;
            this.goalTileY = goalTileY;
            this.health = health;
            this.damage = damage;
            this.timeBetweenAttacks = timeBetweenAttacks;
            this.speed = speed;
        }
    }
}
