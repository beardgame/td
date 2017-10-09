using System.Runtime.InteropServices;

// ReSharper disable ConvertToAutoProperty

namespace Bearded.TD.Game.Units
{
    [StructLayout(LayoutKind.Sequential)]
    struct GameUnitState
    {
        private readonly float x;
        private readonly float y;
        private readonly int health;

        public float X => x;
        public float Y => y;
        public int Health => health;

        public GameUnitState(float x, float y, int health)
        {
            this.x = x;
            this.y = y;
            this.health = health;
        }
    }
}
