using System.Collections.Generic;
using Fody;

namespace Weavers.TechEffects
{
    public sealed class TechEffectWeaver : BaseModuleWeaver, ILogger
    {
        public override void Execute()
        {
            var weaver = new ImplementationWeaver(ModuleDefinition, TypeSystem, this, new ReferenceFinder(ModuleDefinition));
            weaver.Execute();
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield break;
        }
    }
}
