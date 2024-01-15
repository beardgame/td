using Bearded.TD.Content.Mods;

namespace Bearded.TD;

static partial class Constants
{
    public static class Content
    {
        public const string DefaultModId = "default";

        public static class CoreUI
        {
            public const string ModId = "core-ui";

            public static class Fonts
            {
                public static readonly ModAwareId DefaultText = new (ModId, "default-text");
                public static readonly ModAwareId DefaultTextSmall = new (ModId, "default-text-small");
                public static readonly ModAwareId DefaultTitle = new (ModId, "default-title");

                public static readonly ModAwareId DecoTitle = new (ModId, "deco-title");
                public static readonly ModAwareId ScriptTitle = new (ModId, "script-title");
            }
        }
    }
}
