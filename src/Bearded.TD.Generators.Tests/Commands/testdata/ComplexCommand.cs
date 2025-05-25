using System;
using System.Collections.Generic;
using Bearded.TD.Shared.Commands;

namespace Bearded.TD.Generators.Tests.Commands;

[GenerateCommand]
static partial class ComplexCommand
{
    private static void execute(
        MyOwnClass aClass,
        MyOwnStruct aStruct,
        List<MyOwnClass> aListOfClasses,
        List<Dictionary<MyOwnClass, MyOwnStruct>> aListOfDictionaries,
        int anInteger,
        string aString
        )
    {
        throw new NotImplementedException(":)");
    }
}

class MyOwnClass;

struct MyOwnStruct;
