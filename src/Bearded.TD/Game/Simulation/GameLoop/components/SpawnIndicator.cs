using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Game.Simulation.Drawing;
using Bearded.TD.Game.Simulation.Enemies;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Rendering.Vertices;
using Bearded.TD.Shared.Events;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Navigation.NextDirectionFinder;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameLoop;

sealed class SpawnIndicator : Component, IListener<DrawComponents>, IFutureEnemySpawnIndicator
{
    private static readonly Difference3 iconOffsetFromObject = new(0.6f, 0, 0.1f);
    private static readonly Difference3 offsetBetweenIcons = new(0, 0.5f, 0);
    private static readonly Difference3 offsetToText = new(0.25f, 0, 0);
    private const float iconSize = 0.5f;
    private const float fontSize = 0.25f;

    private readonly List<QueuedEnemyForm> futureSpawns = new();
    private readonly Tile tile;
    private Instant nextIndicatorSpawn;
    private Bias nextIndicatorBias;

    public SpawnIndicator(Tile tile)
    {
        this.tile = tile;
    }

    protected override void OnAdded() { }

    public override void Activate()
    {
        Owner.Position = Level.GetPosition(tile).WithZ(Unit.Zero);
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (Owner.Game.Time < nextIndicatorSpawn) return;
        Owner.Game.Add(GameLoopObjectFactory.CreateEnemyPathIndicator(tile, nextIndicatorBias));
        nextIndicatorSpawn = Owner.Game.Time + Constants.Game.Enemy.TimeBetweenIndicators;
        nextIndicatorBias = (Bias) (((int) nextIndicatorBias + 1) % Enum.GetValues(typeof(Bias)).Length);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
        base.OnRemoved();
    }

    public void AddFutureEnemySpawn(EnemyForm form, int amount)
    {
        var instantiatedEnemy = EnemyFactory.Create(Id<GameObject>.Invalid, form, Tile.Origin);
        var icon = instantiatedEnemy.GetComponents<IEnemyIcon>().SingleOrDefault();
        CachedEnemyIcon? cachedSprite = icon?.MakeIconSprite(Owner.Game) is { } sprite && icon.IconColor is var color
            ? new CachedEnemyIcon(sprite, color)
            : null;
        futureSpawns.Add(new QueuedEnemyForm(form, amount, cachedSprite));
    }

    public void FulfilFutureEnemySpawn(EnemyForm form)
    {
        var i = futureSpawns.FindIndex(q => q.Form == form);
        if (i < 0)
        {
            Owner.Game.Meta.Logger.Warning?.Log(
                "Attempted to remove an enemy from spawn indicator which wasn't queued");
            return;
        }

        var spawnsRemaining = futureSpawns[i].Amount;
        if (spawnsRemaining <= 1)
        {
            futureSpawns.RemoveAt(i);
        }
        else
        {
            futureSpawns[i] = futureSpawns[i] with { Amount = spawnsRemaining - 1 };
        }
    }

    public void HandleEvent(DrawComponents @event)
    {
        var futureSpawnsWithIcons = futureSpawns.Where(q => q.Icon is not null).ToImmutableArray();

        if (futureSpawnsWithIcons.Length == 0)
        {
            return;
        }

        var drawer = @event.Drawer;
        var font = @event.Core.InGameFont.With(fontHeight: fontSize, alignHorizontal: 0, alignVertical: 0.5f);
        var anchor =
            Owner.Position + iconOffsetFromObject - 0.5f * (futureSpawnsWithIcons.Length - 1) * offsetBetweenIcons;

        for (var i = 0; i < futureSpawnsWithIcons.Length; i++)
        {
            var icon = futureSpawnsWithIcons[i].Icon!.Value;
            var pos = anchor + i * offsetBetweenIcons;

            drawer.DrawSprite(icon.Sprite, pos.NumericValue, iconSize, icon.Color);
            font.DrawLine(icon.Color, (pos + offsetToText).NumericValue, $"{futureSpawnsWithIcons[i].Amount}");
        }
    }

    private readonly record struct QueuedEnemyForm(EnemyForm Form, int Amount, CachedEnemyIcon? Icon);

    private readonly record struct CachedEnemyIcon(SpriteDrawInfo<UVColorVertex, Color> Sprite, Color Color);
}
