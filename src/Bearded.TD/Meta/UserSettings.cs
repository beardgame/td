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

    [DiscoverableSettingGroup]
    public UISettings UI = new();

    [DiscoverableSettingGroup]
    public GraphicsSettings Graphics = new();

    [DiscoverableSettingGroup(DisplayName = "Miscellaneous")]
    public MiscSettings Misc = new();

    public DebugSettings Debug = new();

    public GameSettings.Builder LastGameSettings = new();

    public sealed class MiscSettings
    {
        [DiscoverableTextSetting]
        public string Username = "";

        [DiscoverableTextSetting(DisplayName = "Master server address")]
        // ReSharper disable once StringLiteralTypo
        public string MasterServerAddress = "tomrijnbeek.me";

        public string SavedNetworkAddress = "";
        public string? ScreenshotPath = null;
    }

    public sealed class UISettings
    {
        [DiscoverableSelectSetting(DisplayName = "UI scale", Options = new object[] { 0.75f, 1f, 1.25f, 1.5f, 2f })]
        public float UIScale = 1f;

        [DiscoverableBoolSetting(DisplayName = "Always show temperature bars")]
        public bool AlwaysShowTemperature = false;
    }

    public sealed class GraphicsSettings
    {
        [DiscoverableSelectSetting(DisplayName = "Super sampling", Options = new object[] { 1f, 2f, 4f, 8f })]
        public float SuperSample = 1f;

        [DiscoverableSelectSetting(DisplayName = "Heightmap resolution", Options = new object[] { 10f, 20f, 40f })]
        public float TerrainHeightmapResolution = 10;

        [DiscoverableSelectSetting(DisplayName = "Terrain resolution", Options = new object[] { 5f, 10f, 20f })]
        public float TerrainMeshResolution = 5;

        public float FOV = 25;

        public float ScreenShake = 1;
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

        public bool ShowTraceMessages = false;

        // simple debug rendering
        public bool Deferred = false;
        public bool RenderUIFallBack = false;
        public bool SimpleFluid = false;
        public bool ProjectileDots = false;
        public bool WireframeLevel = false;
        [SettingOptions(0.5, 1, 2, 10)]
        public float TerrainDetail = 1;
        public bool TowerTargeting = false;
        public bool DamageResistances = false;

        // these don't show up in the game debug ui
        public string DiscordScreenshotWebhookToken = "";
    }
}
