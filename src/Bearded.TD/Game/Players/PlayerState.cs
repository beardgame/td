using System.Runtime.InteropServices;

namespace Bearded.TD.Game.Players
{
    [StructLayout(LayoutKind.Sequential)]
    readonly struct PlayerState
    {
        public byte ConnectionState { get; }
        public int LastKnownPing { get; }

        public PlayerState(
                byte connectionState,
                int lastKnownPing)
        {
            ConnectionState = connectionState;
            LastKnownPing = lastKnownPing;
        }
    }
}
