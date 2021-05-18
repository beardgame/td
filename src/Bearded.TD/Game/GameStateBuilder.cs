using System.Collections.Generic;
using System.Linq;
using Bearded.Graphics;
using Bearded.TD.Commands;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Commands.General;
using Bearded.TD.Game.Commands.Loading;
using Bearded.TD.Game.Generation;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    sealed class GameStateBuilder
    {
        private readonly GameInstance game;
        private readonly GameSettings gameSettings;
        private readonly ILevelGenerator levelGenerator;

        public GameStateBuilder(GameInstance game, ILevelGenerator levelGenerator)
        {
            this.game = game;
            gameSettings = game.GameSettings;
            this.levelGenerator = levelGenerator;
        }

        public IEnumerable<ISerializableCommand<GameInstance>> Generate()
        {
            yield return InitializeTypes.Command();
            yield return CreateGameState.Command(game, gameSettings);
            yield return AddFaction.Command(game, new Faction(
                game.Ids.GetNext<Faction>(),
                game.State,
                name: "All players",
                parent: null,
                hasResources: true,
                hasWorkerNetwork: true,
                hasWorkers: true));
            foreach (var command in setupFactions())
                yield return command;

            var gameMode = game.Blueprints.GameModes[gameSettings.GameMode ?? ModAwareId.ForDefaultMod("default")];
            yield return ApplyGameRules.Command(game, gameMode);

            var nodeAccumulator = new AccumulateNodeGroups.Accumulator();
            game.Meta.Events.Send(new AccumulateNodeGroups(nodeAccumulator));

            var levelGenerationCommands = levelGenerator.Generate(
                new LevelGenerationParameters(gameSettings.LevelSize, nodeAccumulator.ToNodes()), gameSettings.Seed);

            foreach (var commandFactory in levelGenerationCommands)
            {
                yield return commandFactory(game);
            }

            yield return BlockTilesForBuilding.Command(
                    game,
                    Tilemap.GetSpiralCenteredAt(Tile.Origin, 3).ToList());
            yield return TurnCrevicesIntoFluidSinks.Command(game);
            yield return TurnEdgesIntoFluidSinks.Command(game);

            foreach (var command in spawnLocations(gameSettings.LevelSize))
                yield return command;
            foreach (var command in spawnCrystals())
                yield return command;
        }

        private IEnumerable<ISerializableCommand<GameInstance>> spawnLocations(int radius)
        {
            return
                from dir in Directions.All.Enumerate()
                where dir != Direction.Unknown
                select Tile.Origin.Offset(dir.Step() * radius) into tile
                select CreateSpawnLocation.Command(game, game.Ids.GetNext<SpawnLocation>(), tile);
        }

        private IEnumerable<ISerializableCommand<GameInstance>> spawnCrystals()
        {
            var level = game.State.Level;
            var geometry = game.State.GeometryLayer;

            var crystalBlueprints = new[]
                {
                    ModAwareId.ForDefaultMod("crystal-cyan0"),
                    ModAwareId.ForDefaultMod("crystal-cyan1"),
                    ModAwareId.ForDefaultMod("crystal-cyan2"),
                    ModAwareId.ForDefaultMod("crystal-cyan3")
                }
                .Select(id => game.Blueprints.ComponentOwners[id])
                .ToList();

            foreach (var tile in Tilemap.EnumerateTilemapWith(level.Radius - 1))
            {
                var centerGeometry = geometry[tile];
                var height = centerGeometry.DrawInfo.Height;

                if (centerGeometry.Type == TileType.Wall)
                    continue;

                if (centerGeometry.Type == TileType.Floor && StaticRandom.Bool(0.1f))
                {
                    var position = Level.GetPosition(tile)
                        + Direction2.FromDegrees(StaticRandom.Float(360))
                        * StaticRandom.Float(0, Constants.Game.World.HexagonSide * centerGeometry.DrawInfo.HexScale * 0.8f).U();
                    var z = height + 0.01.U();
                    var count = StaticRandom.Int(3, 7);
                    foreach (var _ in Enumerable.Range(0, count))
                    {
                        var direction = Direction2.FromDegrees(StaticRandom.Float(360));
                        var offset = direction * (StaticRandom.Float(2f, 5f) * Constants.Rendering.PixelSize).U();

                        yield return PlopComponentGameObject.Command(game,
                            crystalBlueprints.RandomElement(),
                            (position + offset).WithZ(z),
                            direction
                        );
                    }
                }

                foreach (var direction in Directions.All.Enumerate())
                {
                    var neighbour = tile.Neighbour(direction);
                    var neighbourGeometry = geometry[neighbour];

                    switch (centerGeometry.Type, neighbourGeometry.Type)
                    {
                        case (_, TileType.Floor):
                        case (TileType.Floor, TileType.Wall):
                            break;
                        default:
                            continue;
                    }

                    var neighbourHeight = neighbourGeometry.DrawInfo.Height;

                    if (height + Constants.Game.Navigation.MaxWalkableHeightDifference > neighbourHeight)
                        continue;

                    if (StaticRandom.Bool(0.90))
                        continue;

                    var dirVector = direction.Vector();
                    var dir = -direction.SpaceTimeDirection();

                    var count = centerGeometry.Type == TileType.Crevice
                        ? StaticRandom.Int(3, 12)
                        : StaticRandom.Int(2, 4);


                    var zFactorCenter = centerGeometry.Type == TileType.Crevice
                        ? StaticRandom.Float(0.6f, 0.8f)
                        : StaticRandom.Float(0.025f, 0.3f);

                    foreach (var _ in Enumerable.Range(0, count))
                    {
                        var offset = StaticRandom.Float(-1, 1);
                        var offsetAngle = (offset * 30).Degrees();
                        var offsetPosition = offset * dirVector.PerpendicularRight
                            * Constants.Game.World.HexagonSide * 0.3.U();

                        var zFactor = zFactorCenter * StaticRandom.Float(0.8f, 1.2f);

                        var position = Level.GetPosition(tile) + dirVector
                            * (Constants.Game.World.HexagonWidth * 0.5.U()
                                * Interpolate.Lerp(
                                    centerGeometry.DrawInfo.HexScale,
                                    2 - neighbourGeometry.DrawInfo.HexScale,
                                    zFactor)
                                - Constants.Rendering.PixelSize.U() * 1.5f
                            );

                        var z = height + zFactor * (neighbourHeight - height);

                        yield return PlopComponentGameObject.Command(game,
                            crystalBlueprints.RandomElement(),
                            (position + offsetPosition).WithZ(z),
                            dir + offsetAngle
                            );
                    }
                }
            }
        }

        private IEnumerable<ISerializableCommand<GameInstance>> setupFactions()
        {
            foreach (var (p, i) in game.Players.Indexed())
            {
                var factionColor = Color.FromHSVA(i * MathConstants.TwoPi / 6, 1, 1f);
                var playerFaction = new Faction(
                    game.Ids.GetNext<Faction>(),
                    game.State,
                    parent: game.State.RootFaction,
                    hasResources: false,
                    hasWorkerNetwork: false,
                    hasWorkers: gameSettings.WorkerDistributionMethod != WorkerDistributionMethod.Neutral,
                    name: p.Name,
                    color: factionColor);
                yield return AddFaction.Command(game, playerFaction);
                yield return SetPlayerFaction.Command(p, playerFaction);
            }
        }
    }
}
