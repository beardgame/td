﻿using Bearded.Utilities;
using Newtonsoft.Json;

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

        public UserSettings()
        {
            Dump = this;
        }

        [JsonIgnore]
        public UserSettings Dump;
        
        public MiscSettings Misc = new MiscSettings();
        public UISettings UI = new UISettings();
        public GraphicsSettings Graphics = new GraphicsSettings();
        public DebugSettings Debug = new DebugSettings();
        
        public class MiscSettings
        {
            public string Username = "";
            public string SavedNetworkAddress = "";
            public string MasterServerAddress = "localhost";

            public bool ShowTraceMessages = true;
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
            public int Pathfinding = 0;
            public int InfoScreen = 0;
            public bool InvulnerableBuildings = false;
            public double GameSpeed = 1;
        }
    }
}
