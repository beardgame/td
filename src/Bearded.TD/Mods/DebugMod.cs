using amulware.Graphics;
using Bearded.TD.Mods.Models;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods
{
    static class DebugMod
    {
        private static readonly IdManager ids = new IdManager();

        public static Mod Create()
        {
            var enemies = new[]
            {
                new UnitBlueprint(
                    ids.GetNext<UnitBlueprint>(), "debug", 100, 10, 2.S(), new Speed(2), 2, Color.DarkRed),
                new UnitBlueprint(
                    ids.GetNext<UnitBlueprint>(), "strong", 250, 20, 1.5.S(), new Speed(1.2f), 4, Color.Yellow),
                new UnitBlueprint(
                    ids.GetNext<UnitBlueprint>(), "fast", 50, 4, .5.S(), new Speed(3), 4, Color.CornflowerBlue),
                new UnitBlueprint(
                    ids.GetNext<UnitBlueprint>(), "tank", 1000, 50, 2.S(), new Speed(.8f), 12, Color.SandyBrown)
            };

            return new Mod(
                new ReadonlyNamedBlueprintCollection<UnitBlueprint>(enemies));
        }
    }
}
