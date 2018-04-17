using System.Runtime.InteropServices;

namespace Bearded.TD.Game.Players
{
    [StructLayout(LayoutKind.Sequential)]
    struct PlayerState
    {
        public byte ConnectionState { get; }
        public float LastKnownPing { get; }

        public PlayerState(
                byte connectionState,
                float lastKnownPing)
        {
            ConnectionState = connectionState;
            LastKnownPing = lastKnownPing;
        }
    }
}
