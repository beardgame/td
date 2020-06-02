using Bearded.TD.Game;
using Bearded.Utilities;
using Newtonsoft.Json;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToConstant.Global

namespace Bearded.TD.Meta
{
    sealed partial class UserSettings
    {
        public static UserSettings Instance { get; private set; }

        public static event VoidEventHandler? SettingsChanged;

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
            public float SuperSample = 1f;
        }

        public class DebugSettings
        {
            public bool GameDebugScreen = false;

            // most useful game debug settings
            [SettingOptions(0.1, 0.5, 1, 2, 10)]
            public double GameSpeed = 1;
            public bool InvulnerableBuildings = false;

            // debug rendering for more complex systems
            public bool LevelMetadata = false;
            public bool LevelGeometry = false;
            public bool LevelGeometryShowHeights = false;
            public bool LevelGeometryLabels = false;
            [SettingOptions(0, 1, 2)]
            public int Coordinates = 0;
            [SettingOptions(0, 1, 2)]
            public int Pathfinding = 0;
            public bool Passability = false;

            // simple debug rendering
            public bool Deferred = false;
            public bool RenderUIFallBack = false;
            public bool SimpleFluid = false;
            public bool ProjectileDots = false;
            public bool WireframeLevel = false;
            [SettingOptions(0.5, 1, 2, 10)]
            public float TerrainDetail = 1;
        }
    }
}
