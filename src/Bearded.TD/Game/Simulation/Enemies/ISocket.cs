namespace Bearded.TD.Game.Simulation.Enemies;

interface ISocket
{
    SocketShape Shape { get; }
    void Fill(IModule module);
}
