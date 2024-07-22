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

            public static class DefaultShaders
            {
                public static ModAwareId Sprite = new (ModId, "default-sprite");
                public static ModAwareId Shapes = new (ModId, "shapes");
            }

            public static class Fonts
            {
                public static readonly ModAwareId DefaultText = new (ModId, "default-text");
                public static readonly ModAwareId DefaultTextSmall = new (ModId, "default-text-small");
                public static readonly ModAwareId DefaultTitle = new (ModId, "default-title");

                public static readonly ModAwareId DecoTitle = new (ModId, "deco-title");
                public static readonly ModAwareId ScriptTitle = new (ModId, "script-title");

                public static readonly ModAwareId MonospaceText = new (ModId, "monospace-text");
            }

            public static class Sprites
            {
                private static ModAwareSpriteId symbol(string id) => new(new ModAwareId(ModId, "symbols"), id);

                public static readonly ModAwareSpriteId CheckMark = symbol("checkmark");
                public static readonly ModAwareSpriteId QuestionMark = symbol("question-mark");
                public static readonly ModAwareSpriteId Star = symbol("star");
                public static readonly ModAwareSpriteId HexWreath = symbol("hex-wreath");

                private static ModAwareSpriteId hud(string id) => new(new ModAwareId(ModId, "hud"), id);

                public static readonly ModAwareSpriteId Technology = hud("microscope-lens");
                public static readonly ModAwareSpriteId GridNone = hud("grid-none");
                public static readonly ModAwareSpriteId GridLines = hud("grid-lines");
                public static readonly ModAwareSpriteId GridFill = hud("grid-fill");

                public static ModAwareSpriteId Targeting(string id) => new(new ModAwareId(ModId, "targeting"), id);
            }

            public static class MainMenu
            {
                private static ModAwareSpriteId foreground(string id) => new(new ModAwareId(ModId, "menu-foreground"), id);

                public static readonly ModAwareSpriteId Turret = foreground("turret");
                public static readonly ModAwareId CaveBackground = new (ModId, "menu-background");
            }

            public static class Sounds
            {
                public static readonly ModAwareId UpgradeGeneric = new(ModId, "clank");
                public static readonly ModAwareId UpgradeKinetics = new(ModId, "upgrade-kinetics");
                public static readonly ModAwareId UpgradeFire = new(ModId, "upgrade-fire");
                public static readonly ModAwareId UpgradeLightning = new(ModId, "upgrade-lightning");
            }
        }
    }
}
