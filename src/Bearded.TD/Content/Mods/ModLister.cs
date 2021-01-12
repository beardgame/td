using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Bearded.TD.Content.Serialization.Models;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Content.Mods
{
    sealed class ModLister
    {
        public List<ModMetadata> GetAll()
        {
            var path = Constants.Paths.Content.Asset("mods/");

            var dir = new DirectoryInfo(path);

            if (!dir.Exists)
                throw new ArgumentException($"Mod path '{path}' does not exist.");

            return dir
                .EnumerateDirectories()
                .Select(d => d.GetFiles("mod.json").SingleOrDefault())
                .NotNull()
                .Select(load)
                .ToList();
        }

        private ModMetadata load(FileInfo modFile)
        {
            var meta = JsonSerializer.Deserialize<Metadata>(
                File.ReadAllText(modFile.FullName), Constants.Serialization.DefaultJsonSerializerOptions);
            if (meta == null)
            {
                throw new InvalidDataException("Metadata was not parsed correctly");
            }

            return new ModMetadata(meta, modFile.Directory);
        }
    }
}
