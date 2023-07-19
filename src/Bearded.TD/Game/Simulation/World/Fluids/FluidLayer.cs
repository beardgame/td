namespace Bearded.TD.Game.Simulation.World.Fluids;

sealed class FluidLayer
{
    private readonly GameState gameState;
    public Fluid Water { get; }

    public FluidLayer(GameState gameState, GeometryLayer geometryLayer, int radius)
    {
        this.gameState = gameState;
        Water = new Fluid(geometryLayer, radius, 10);
    }

    public void Update()
    {
        Water.Update(gameState.Time);
    }
}