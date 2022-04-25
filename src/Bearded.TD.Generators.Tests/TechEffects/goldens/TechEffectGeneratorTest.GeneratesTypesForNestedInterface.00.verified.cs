﻿//HintName: ContainingTypeNestedParametersModifiable.cs
// This file is generated by Bearded.TD.Generators

using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;

namespace Bearded.TD.Generators.Tests.TechEffects
{
    [ParametersModifiable(typeof(ContainingType.INestedParameters))]
    sealed class ContainingTypeNestedParametersModifiable : ModifiableBase<ContainingTypeNestedParametersModifiable>, ContainingType.INestedParameters
    {
        private readonly ContainingType.INestedParameters template;

        public int MyInt => template.MyInt;

        public ContainingTypeNestedParametersModifiable(ContainingType.INestedParameters template)
        {
            this.template = template;

        }

        static ContainingTypeNestedParametersModifiable()
        {
            InitializeAttributes(
                new (AttributeType Type, Func<ContainingTypeNestedParametersModifiable, IAttributeWithModifications> Getter)[]{
                }.ToLookup(tuple => tuple.Type, tuple => tuple.Getter));
        }

        public ContainingType.INestedParameters CreateModifiableInstance() => new ContainingTypeNestedParametersModifiable(template);

    }
}