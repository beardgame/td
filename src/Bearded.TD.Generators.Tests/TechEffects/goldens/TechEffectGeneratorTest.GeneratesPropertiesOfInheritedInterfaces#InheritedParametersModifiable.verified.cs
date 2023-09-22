﻿//HintName: InheritedParametersModifiable.cs
// This file is generated by Bearded.TD.Generators

using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;

namespace Bearded.TD.Generators.Tests.TechEffects
{
    [ParametersModifiable(typeof(IInheritedParameters))]
    sealed class InheritedParametersModifiable : ModifiableBase<InheritedParametersModifiable>, IInheritedParameters
    {
        private readonly IInheritedParameters template;

        public int LocalParameter => template.LocalParameter;

        public double BaseParameter => template.BaseParameter;

        public InheritedParametersModifiable(IInheritedParameters template)
        {
            this.template = template;

        }

        static InheritedParametersModifiable()
        {
            InitializeAttributes(
                new (AttributeType Type, Func<InheritedParametersModifiable, IAttributeWithModifications> Getter)[]{
                }.ToLookup(tuple => tuple.Type, tuple => tuple.Getter));
        }

        public IInheritedParameters CreateModifiableInstance() => new InheritedParametersModifiable(template);

    }
}
