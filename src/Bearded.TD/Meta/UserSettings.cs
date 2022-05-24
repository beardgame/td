using Bearded.TD.Game;
using Bearded.Utilities;
using JetBrains.Annotations;
using Newtonsoft.Json;
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ConvertToConstant.Global

namespace Bearded.TD.Meta;

sealed partial class UserSettings
{
    public static UserSettings Instance { get; private set; } = getDefaultInstance();

    public static event VoidEventHandler? SettingsChanged;

    public static void RaiseSettingsChanged() => SettingsChanged?.Invoke();

    // ReSharper disable once MemberCanBePrivate.Global
    public UserSettings()
    {
        Dump = this;
    }

    [JsonIgnore]
    [UsedImplicitly]
    public UserSettings Dump;

    public MiscSettings Misc = new();
    public UISettings UI = new();
    public GraphicsSettings Graphics = new();
    public DebugSettings Debug = new();
    public GameSettings.Builder LastGameSettings = new();

    public sealed class MiscSettings
    {
        public string Username = "";
        public string SavedNetworkAddress = "";

        public bool ShowTraceMessages = true;
        public string MasterServerAddress = "tomrijnbeek.me";

        public string? ScreenshotPath = null;
    }

    public sealed class UISettings
    {
        public float UIScale = 1f;
    }

    public sealed class GraphicsSettings
    {
        public float SuperSample = 1f;

        public float TerrainHeightmapResolution = 10;
        public float TerrainMeshResolution = 5;
        public float FOV = 25;
    }

    public sealed class DebugSettings
    {
        public bool GameDebugScreen = false;

        public bool PerformanceOverlay = false;

        // most useful game debug settings
        [SettingOptions(0.1, 0.5, 1, 2, 10)]
        public double GameSpeed = 1;
        public bool InvulnerableBuildings = false;

        // debug rendering for more complex systems
        public bool Zones = false;
        public bool Visibility = false;
        public bool LevelMetadata = false;
        public bool LevelGeometry = false;
        public bool LevelGeometryShowHeights = false;
        public bool LevelGeometryLabels = false;
        [SettingOptions(0, 1, 2)]
        public int Coordinates = 0;
        [SettingOptions(0, 1, 2)]
        public int Pathfinding = 0;
        public bool Passability = false;
        [SettingOptions(0, 1, 2)]
        public int DebugPathfinder = 0;

        // simple debug rendering
        public bool Deferred = false;
        public bool RenderUIFallBack = false;
        public bool SimpleFluid = false;
        public bool ProjectileDots = false;
        public bool WireframeLevel = false;
        [SettingOptions(0.5, 1, 2, 10)]
        public float TerrainDetail = 1;

        // these don't show up in the game debug ui
        public string DiscordScreenshotWebhookToken = "";
    }
}
