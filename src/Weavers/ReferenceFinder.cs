using System;
using System.Linq;
using System.Linq.Expressions;
using Mono.Cecil;

namespace Weavers
{
    sealed class ReferenceFinder
    {
        private readonly ModuleDefinition moduleDefinition;
        private readonly Func<string, TypeDefinition> findType;

        internal ReferenceFinder(ModuleDefinition moduleDefinition, Func<string, TypeDefinition> findType)
        {
            this.moduleDefinition = moduleDefinition;
            this.findType = findType;
        }

        internal FieldReference GetFieldReference(TypeReference typeReference, string name)
            => GetFieldReference(typeReference, field => field.Name == name);

        internal FieldReference GetFieldReference(TypeReference typeReference, Func<FieldDefinition, bool> predicate)
        {
            var typeDefinition = resolve(typeReference);

            FieldDefinition fieldDefinition;
            do
            {
                fieldDefinition = typeDefinition.Fields.FirstOrDefault(predicate);
                typeDefinition = typeDefinition.BaseType == null ? null : resolve(typeDefinition.BaseType);
            } while (fieldDefinition == null && typeDefinition != null);

            return moduleDefinition.ImportReference(fieldDefinition);
        }

        internal PropertyReference GetPropertyReference(TypeReference typeReference, string name)
            => GetPropertyReference(typeReference, field => field.Name == name);

        internal PropertyReference GetPropertyReference(TypeReference typeReference, Func<PropertyDefinition, bool> predicate)
        {
            var typeDefinition = resolve(typeReference);

            PropertyDefinition propertyDefinition;
            do
            {
                propertyDefinition = typeDefinition.Properties.FirstOrDefault(predicate);
                typeDefinition = typeDefinition.BaseType == null ? null : resolve(typeDefinition.BaseType);
            } while (propertyDefinition == null && typeDefinition != null);

            return propertyDefinition;
        }

        internal MethodReference GetConstructorReference(Type declaringType)
        {
            return GetMethodReference(declaringType, method => method.IsConstructor);
        }

        internal MethodReference GetConstructorReference(TypeReference typeReference)
        {
            return GetMethodReference(typeReference, method => method.IsConstructor);
        }

        internal MethodReference GetMethodReference<T>(Expression<Action<T>> expression)
        {
            if (expression.Body is MethodCallExpression methodCall)
            {
                return moduleDefinition.ImportReference(methodCall.Method);
            }
            throw new ArgumentException("The expression must be a method call.", nameof(expression));
        }

        internal MethodReference GetMethodReference(Type declaringType, Func<MethodDefinition, bool> predicate)
        {
            return GetMethodReference(GetTypeReference(declaringType), predicate);
        }

        internal MethodReference GetMethodReference(TypeReference typeReference, string name)
            => GetMethodReference(typeReference, method => method.Name == name);

        internal MethodReference GetMethodReference(TypeReference typeReference, Func<MethodDefinition, bool> predicate)
        {
            var typeDefinition = resolve(typeReference);
            var methodDefinition = typeDefinition.Methods.FirstOrDefault(predicate);
            if (methodDefinition == null)
            {
                throw new InvalidOperationException("Cannot find valid method.");
            }
            var importedMethod = moduleDefinition.ImportReference(methodDefinition);
            // set the declaring type to the original type in case we lost the generic parameters
            importedMethod.DeclaringType = moduleDefinition.ImportReference(typeReference);
            return importedMethod;
        }

        internal TypeReference GetTypeReference<T>() => GetTypeReference(typeof(T));

        internal TypeReference GetTypeReference(Type type)
        {
            return moduleDefinition.ImportReference(type);
        }

        private TypeDefinition resolve(TypeReference typeReference)
        {
            return typeReference.Resolve() ?? findType(typeReference.FullName.Split('<')[0]);
        }
    }
}
