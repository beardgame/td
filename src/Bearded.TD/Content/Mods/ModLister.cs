using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities.Linq;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Mods
{
    sealed class ModLister
    {
        public List<ModMetadata> GetAll(string path = "assets/mods/")
        {
            var dir = new DirectoryInfo(path);

            #if DEBUG

            dir = new DirectoryInfo(
                SurfaceManager.AdjustPathToReloadable(dir.FullName)
            );

            #endif

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
            var meta = JsonConvert.DeserializeObject<Metadata>(File.ReadAllText(modFile.FullName));

            return new ModMetadata(meta, modFile.Directory);
        }
    }
}
