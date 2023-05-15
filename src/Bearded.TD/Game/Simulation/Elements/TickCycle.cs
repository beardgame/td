using System;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Constants.Game.Elements;

namespace Bearded.TD.Game.Simulation.Elements;

sealed class TickCycle
{
    private readonly GameState game;
    private readonly Action<Instant> doTick;
    private Instant? lastTick;

    public TickCycle(GameState game, Action<Instant> doTick)
    {
        this.game = game;
        this.doTick = doTick;
    }

    public void Update()
    {
        if (lastTick is null)
        {
            lastTick = game.Time;
            return;
        }

        while (game.Time - lastTick >= TickDuration)
        {
            var now = lastTick.Value + TickDuration;
            doTick(now);
            lastTick = now;
        }
    }
}
