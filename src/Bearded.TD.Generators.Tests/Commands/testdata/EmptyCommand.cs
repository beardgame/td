using System;
using Bearded.TD.Shared.Commands;

namespace Bearded.TD.Generators.Tests.Commands;

[GenerateCommand]
static partial class EmptyCommand
{
    private static void execute()
    {
        throw new NotImplementedException(":)");
    }
}
