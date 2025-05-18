using System;
using Bearded.TD.Shared.Commands;

namespace Bearded.TD.Generators.Tests.Commands;

[GenerateCommand]
static partial class SimpleCommand
{
    private static void execute(int anInteger, string aString)
    {
        throw new NotImplementedException(":)");
    }
}
