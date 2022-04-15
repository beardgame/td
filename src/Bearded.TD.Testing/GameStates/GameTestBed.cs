using Bearded.TD.Game.Simulation;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Testing.GameStates;

sealed class GameTestBed
{
    private static readonly TimeSpan frameTime = TimeSpan.One / 60f;

    public GameState State { get; }

    private GameTestBed(GameState state)
    {
        State = state;
    }

    public void AdvanceFramesFor(TimeSpan duration)
    {
        var numFrames = MoreMath.CeilToInt(duration / frameTime);
        foreach (var _ in Enumerable.Range(0, numFrames))
        {
            AdvanceSingleFrame();
        }
    }

    public void AdvanceSingleFrame()
    {
        AdvanceAtOnce(frameTime);
    }

    public void AdvanceAtOnce(TimeSpan time)
    {
        State.Advance(time);
    }

    public static GameTestBed Create()
    {
        return new GameTestBed(GameStateTestFactory.Create());
    }
}
