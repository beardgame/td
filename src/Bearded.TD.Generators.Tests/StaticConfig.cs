using System.Runtime.CompilerServices;
using DiffEngine;
using VerifyTests;

namespace Bearded.TD.Generators.Tests
{
    public static class StaticConfig
    {
        public static readonly VerifySettings DefaultVerifySettings;

        static StaticConfig()
        {
            DefaultVerifySettings = new VerifySettings();
            DefaultVerifySettings.UseDirectory("goldens");
        }

        [ModuleInitializer]
        public static void Initialize()
        {
            DiffTools.UseOrder(DiffTool.Rider, DiffTool.VisualStudioCode);
            VerifySourceGenerators.Enable();
        }
    }
}
