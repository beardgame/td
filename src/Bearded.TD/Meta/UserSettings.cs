using Bearded.TD.Game;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Newtonsoft.Json;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToConstant.Global

namespace Bearded.TD.Meta
{
    sealed partial class UserSettings
    {
        public static UserSettings Instance { get; private set; }

        public static event VoidEventHandler SettingsChanged;

        public static void RaiseSettingsChanged() => SettingsChanged?.Invoke();

        static UserSettings()
        {
            initialiseCommandParameters();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public UserSettings()
        {
            Dump = this;
        }

        [JsonIgnore]
        // ReSharper disable once MemberCanBePrivate.Global
        public UserSettings Dump;
        
        public MiscSettings Misc = new MiscSettings();
        public UISettings UI = new UISettings();
        public GraphicsSettings Graphics = new GraphicsSettings();
        public DebugSettings Debug = new DebugSettings();
        public GameSettings.Builder LastGameSettings = new GameSettings.Builder();
        
        public class MiscSettings
        {
            public string Username = "";
            public string SavedNetworkAddress = "";
            public string MasterServerAddress = "localhost";

            public bool ShowTraceMessages = true;

            public int? MapGenSeed = null;
        }

        public class UISettings
        {
            public float UIScale = 1f;
        }

        public class GraphicsSettings
        {
            public float UpSample = 1f;
        }

        public class DebugSettings
        {
            public bool Deferred = false;
            public int Coordinates = 0;
            public int Pathfinding = 0;
            public bool InvulnerableBuildings = false;
            public double GameSpeed = 1;
            public bool RenderUIFallBack = false;
            public LevelGenerator LevelGenerator = LevelGenerator.Default;
        }

        public enum LevelGenerator
        {
            Default,
            Legacy,
            Perlin
        }
    }
}
