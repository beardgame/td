using System;
using Fody;
using Weavers.TechEffects;
#pragma warning disable 618 // Disable obsolete warnings

namespace Weavers.Tests.TechEffects
{
    static class TechEffectWeaverAssembly
    {
        internal static readonly TestResult TestResult;

        static TechEffectWeaverAssembly()
        {
            var weavingTask = new TechEffectWeaver();
            TestResult = weavingTask.ExecuteTestRun(Constants.AssemblyToProcess);
        }
        
        internal static Type GetAssemblyType(string name)
        {
            return TestResult.Assembly.GetType($"{Constants.NameSpace}.{name}");
        }
    }
}
