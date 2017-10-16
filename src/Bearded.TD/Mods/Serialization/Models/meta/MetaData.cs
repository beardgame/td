﻿using System.Collections.Generic;

namespace Bearded.TD.Mods.Serialization.Models
{
    sealed class MetaData
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public List<ModDependency> Dependencies { get; set; }
    }
}
