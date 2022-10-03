using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Bearded.TD.Content.Serialization.Models;
using Bearded.Utilities.Linq;
using Newtonsoft.Json;

namespace Bearded.TD.Content.Mods;

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
            .Select(findModJsonFile)
            .NotNull()
            .Select(load)
            .ToList();
    }

    private static FileInfo? findModJsonFile(DirectoryInfo dir)
    {
        var supportedExtensions = ImmutableHashSet.Create(".json", ".json5");
        return dir.GetFiles()
            .SingleOrDefault(f => supportedExtensions.Contains(f.Extension) &&
                Path.GetFileNameWithoutExtension(f.Name) == "mod");
    }

    private ModMetadata load(FileInfo modFile)
    {
        var meta = JsonConvert.DeserializeObject<Metadata>(File.ReadAllText(modFile.FullName));

        return new ModMetadata(meta, modFile.Directory);
    }
}
