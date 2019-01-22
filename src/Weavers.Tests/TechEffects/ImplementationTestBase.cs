using System;
using System.Reflection;
using FluentAssertions;
using Weavers.Tests.AssemblyToProcess;
using Xunit;
using static Weavers.Tests.TechEffects.TechEffectWeaverAssembly;

namespace Weavers.Tests.TechEffects
{
    public abstract class ImplementationTestBase
    {
        #region Helpers
        
        protected static readonly Type TemplateType = GetAssemblyType("DummyParametersTemplate");
        protected static readonly Type ModifiableType = GetAssemblyType("DummyParametersModifiable");
        protected static readonly Type InterfaceType = GetAssemblyType(nameof(IDummyParametersTemplate));
        protected static readonly Type TechEffectLibraryType = GetAssemblyType(nameof(ParametersTemplateLibrary));
        protected static readonly Type WrappedIntType = GetAssemblyType(nameof(WrappedInt));

        protected static ConstructorInfo TemplateConstructorInfo
        {
            get
            {
                var cs = TemplateType.GetConstructors();
                return cs.Length == 1 ? cs[0] : null;
            }
        }

        protected static ConstructorInfo ModifiableConstructorInfo
        {
            get
            {
                var cs = ModifiableType.GetConstructors();
                return cs.Length == 1 ? cs[0] : null;
            }
        }
        
        protected abstract Type ImplementationType { get; }

        protected static object ConstructTemplate(params object[] constructorParams)
            => TemplateConstructorInfo.Invoke(constructorParams);

        protected static object ConstructModifiable(object template)
            => ModifiableConstructorInfo.Invoke(new[] { template });
        
        #endregion
        
        #region Tests
        
        [Fact]
        public void InjectsTemplateType()
        {
            ImplementationType.Should().NotBeNull();
        }

        [Fact]
        public void MakesTemplateTypeImplementInterface()
        {
            ImplementationType.Should().BeAssignableTo(InterfaceType);
        }

        [Fact]
        public void MakesTemplateTypeConstructable()
        {
            ImplementationType.Should().NotBeNull();
        }
        
        #endregion
    }
}
