namespace Bearded.TD.Game.Simulation.Drones;

sealed class DroneFulfillment
{
    private readonly Drone drone;

    public DroneFulfillment(Drone drone)
    {
        this.drone = drone;
    }

    public void Cancel()
    {
        drone.Cancel();
    }
}
