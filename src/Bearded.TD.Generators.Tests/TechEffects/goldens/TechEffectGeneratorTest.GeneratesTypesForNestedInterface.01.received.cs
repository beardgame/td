﻿//HintName: ContainingTypeNestedParametersTemplate.cs
// This file is generated by Bearded.TD.Generators

using System;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Newtonsoft.Json;

namespace Bearded.TD.Generators.Tests.TechEffects
{
    [ParametersTemplate(typeof(ContainingType.INestedParameters))]
    sealed class ContainingTypeNestedParametersTemplate : TemplateBase, ContainingType.INestedParameters
    {
        public int MyInt { get; }

        [JsonConstructor]
        public ContainingTypeNestedParametersTemplate(
            int myInt)
        {
            MyInt = myInt;
        }

        public bool HasAttributeOfType(AttributeType type) => ContainingTypeNestedParametersModifiable.AttributeIsKnown(type);

        public ContainingType.INestedParameters CreateModifiableInstance() => new ContainingTypeNestedParametersModifiable(this);

    }
}