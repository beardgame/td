using System;
using System.IO;
using Bearded.TD.Mods.Models;
using Newtonsoft.Json;
using Void = Bearded.Utilities.Void;

namespace Bearded.TD.Mods
{
    internal class SpriteSetLoader
    {
        private readonly ModLoadingContext context;
        private readonly ModMetadata meta;
        private readonly JsonSerializer serializer;

        public SpriteSetLoader(ModLoadingContext context, ModMetadata meta, JsonSerializer serializer)
        {
            this.context = context;
            this.meta = meta;
            this.serializer = serializer;
        }

        public SpriteSet TryLoad(FileInfo file)
        {
            try
            {
                var text = file.OpenText();
                var reader = new JsonTextReader(text);
                var jsonModel = serializer.Deserialize<Serialization.Models.SpriteSet>(reader);
                
                // TODO: load png files/inject png file loader into ToGameModel

                var gameModel = jsonModel.ToGameModel(default(Void));

                return gameModel;
            }
            catch (Exception e)
            {
                context.Logger.Error?.Log($"Error loading '{meta.Id}/gfx/sprites/../{file.Name}': {e.Message}");

                return null;
            }
        }
    }
}