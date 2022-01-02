using System;
using System.Collections.Generic;
using System.Linq;

namespace Bearded.TD.Content.Mods;

sealed class ModSorter
{
    public List<ModMetadata> SortByDependency(IEnumerable<ModMetadata> mods)
    {
        var queue = new Queue<ModMetadata>(mods);
        var sortedIds = new HashSet<string>();

        var sorted = new List<ModMetadata>(queue.Count);
        var skipped = 0;

        while (queue.Count > 0)
        {
            var mod = queue.Dequeue();

            if (mod.Dependencies.All(d => sortedIds.Contains(d.Id)))
            {
                sorted.Add(mod);
                sortedIds.Add(mod.Id);
                skipped = 0;
            }
            else
            {
                queue.Enqueue(mod);
                skipped++;

                if (skipped == queue.Count)
                    throw new Exception($"Found circular or unknown mod dependencies, involving '{mod.Id}'.");
            }
        }

        return sorted;
    }
}