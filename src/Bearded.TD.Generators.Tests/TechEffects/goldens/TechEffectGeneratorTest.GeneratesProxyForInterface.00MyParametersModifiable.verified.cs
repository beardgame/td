﻿//HintName: MyParametersModifiable.cs
// This file is generated by Bearded.TD.Generators

using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;

namespace Bearded.TD.Generators.Tests.TechEffects
{
    [ParametersModifiable(typeof(IMyParameters))]
    sealed class MyParametersModifiable : ModifiableBase<MyParametersModifiable>, IMyParameters
    {
        private readonly IMyParameters template;

        public int RawType => template.RawType;

        public int? NullableType => template.NullableType;

        public Unit WrappedType => template.WrappedType;

        private readonly AttributeWithModifications<double> rawTypeWithDefault;
        public double RawTypeWithDefault => rawTypeWithDefault.Value;

        private readonly AttributeWithModifications<bool> boolTypeWithDefault;
        public bool BoolTypeWithDefault => boolTypeWithDefault.Value;

        private readonly AttributeWithModifications<int> modifiableRawType;
        public int ModifiableRawType => modifiableRawType.Value;

        private readonly AttributeWithModifications<Unit> modifiableWrappedType;
        public Unit ModifiableWrappedType => modifiableWrappedType.Value;

        public MyParametersModifiable(IMyParameters template)
        {
            this.template = template;

            rawTypeWithDefault = new AttributeWithModifications<double>(
                template.RawTypeWithDefault,
                x => x);

            boolTypeWithDefault = new AttributeWithModifications<bool>(
                Convert.ToDouble(template.BoolTypeWithDefault),
                x => Convert.ToBoolean(x));

            modifiableRawType = new AttributeWithModifications<int>(
                template.ModifiableRawType,
                x => (int) x);

            modifiableWrappedType = new AttributeWithModifications<Unit>(
                Bearded.TD.Generators.Tests.TechEffects.IMyParameters.UnitConverter.ToRaw(template.ModifiableWrappedType),
                Bearded.TD.Generators.Tests.TechEffects.IMyParameters.UnitConverter.ToWrapped);
        }

        static MyParametersModifiable()
        {
            InitializeAttributes(
                new (AttributeType Type, Func<MyParametersModifiable, IAttributeWithModifications> Getter)[]{
                    (AttributeType.None, instance => instance.rawTypeWithDefault),
                    (AttributeType.None, instance => instance.boolTypeWithDefault),
                    ((AttributeType) 1, instance => instance.modifiableRawType),
                    ((AttributeType) 8, instance => instance.modifiableWrappedType)
                }.ToLookup(tuple => tuple.Type, tuple => tuple.Getter));
        }

        public IMyParameters CreateModifiableInstance() => new MyParametersModifiable(template);

    }
}
