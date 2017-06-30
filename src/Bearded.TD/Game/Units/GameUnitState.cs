using System.Runtime.InteropServices;
// ReSharper disable ConvertToAutoProperty

namespace Bearded.TD.Game.Units
{
    [StructLayout(LayoutKind.Sequential)]
    struct GameUnitState
    {
        private readonly int tileX;
        private readonly int tileY;
        private readonly byte direction;
        private readonly float movementProgress;
        private readonly int health;

        public int TileX => tileX;
        public int TileY => tileY;
        public byte Direction => direction;
        public float MovementProgress => movementProgress;
        public int Health => health;

        public GameUnitState(int tileX, int tileY, byte direction, float movementProgress, int health)
        {
            this.tileX = tileX;
            this.tileY = tileY;
            this.direction = direction;
            this.movementProgress = movementProgress;
            this.health = health;
        }
    }
}
