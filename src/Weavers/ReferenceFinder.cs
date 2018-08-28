﻿using System;
using System.Linq;
using Mono.Cecil;

namespace Weavers
{
    sealed class ReferenceFinder
    {
        private readonly ModuleDefinition moduleDefinition;

        internal ReferenceFinder(ModuleDefinition moduleDefinition)
        {
            this.moduleDefinition = moduleDefinition;
        }

        internal MethodReference GetMethodReference(Type declaringType, Func<MethodDefinition, bool> predicate)
        {
            return GetMethodReference(GetTypeReference(declaringType), predicate);
        }

        internal MethodReference GetMethodReference(TypeReference typeReference, Func<MethodDefinition, bool> predicate)
        {
            var typeDefinition = typeReference.Resolve();

            MethodDefinition methodDefinition;
            do
            {
                methodDefinition = typeDefinition.Methods.FirstOrDefault(predicate);
                typeDefinition = typeDefinition.BaseType?.Resolve();
            } while (methodDefinition == null && typeDefinition != null);

            return moduleDefinition.ImportReference(methodDefinition);
        }

        internal TypeReference GetTypeReference(Type type)
        {
            return moduleDefinition.ImportReference(type);
        }
    }
}
