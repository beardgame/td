using System;
using Bearded.TD.Shared.Commands;

namespace Bearded.TD.Generators.Tests.Commands;

[GenerateCommand]
static partial class ComplexCommand
{
    private static void execute(
        MyOwnClass aClass,
        MyOwnStruct aStruct,
        int anInteger,
        string aString
        )
    {
        throw new NotImplementedException(":)");
    }
}

class MyOwnClass;

struct MyOwnStruct;
