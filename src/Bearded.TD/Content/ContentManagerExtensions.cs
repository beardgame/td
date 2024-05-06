using System;
using Bearded.TD.Audio;
using Bearded.TD.Content.Models;
using Bearded.TD.Content.Mods;

namespace Bearded.TD.Content;

static class ContentManagerExtensions
{
    public static SpriteSet ResolveSpriteSet(this ContentManager contentManager, ModAwareId id)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException("Invalid ID for sprite set", nameof(id));
        }
        return contentManager.GetModUnsafe(id.ModId).Blueprints.Sprites[id];
    }

    public static Material ResolveMaterial(this ContentManager contentManager, ModAwareId id)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException("Invalid ID for material", nameof(id));
        }
        return contentManager.GetModUnsafe(id.ModId).Blueprints.Materials[id];
    }

    public static ISoundEffect ResolveSoundEffect(this ContentManager contentManager, ModAwareId id)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException("Invalid ID for sound effect", nameof(id));
        }
        return contentManager.GetModUnsafe(id.ModId).Blueprints.SoundEffects[id];
    }
}
