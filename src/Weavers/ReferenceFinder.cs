using System;
using System.Linq;
using System.Linq.Expressions;
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

        internal FieldReference GetFieldReference(TypeReference typeReference, string name)
            => GetFieldReference(typeReference, field => field.Name == name);
        
        internal FieldReference GetFieldReference(TypeReference typeReference, Func<FieldDefinition, bool> predicate)
        {
            var typeDefinition = typeReference.Resolve();

            FieldDefinition fieldDefinition;
            do
            {
                fieldDefinition = typeDefinition.Fields.FirstOrDefault(predicate);
                typeDefinition = typeDefinition.BaseType?.Resolve();
            } while (fieldDefinition == null && typeDefinition != null);

            return moduleDefinition.ImportReference(fieldDefinition);
        }

        internal PropertyReference GetPropertyReference(TypeReference typeReference, string name)
            => GetPropertyReference(typeReference, field => field.Name == name);
        
        internal PropertyReference GetPropertyReference(TypeReference typeReference, Func<PropertyDefinition, bool> predicate)
        {
            var typeDefinition = typeReference.Resolve();

            PropertyDefinition propertyDefinition;
            do
            {
                propertyDefinition = typeDefinition.Properties.FirstOrDefault(predicate);
                typeDefinition = typeDefinition.BaseType?.Resolve();
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
            var typeDefinition = typeReference.Resolve();

            MethodDefinition methodDefinition;
            do
            {
                methodDefinition = typeDefinition.Methods.FirstOrDefault(predicate);
                typeDefinition = typeDefinition.BaseType?.Resolve();
            } while (methodDefinition == null && typeDefinition != null);

            return moduleDefinition.ImportReference(methodDefinition);
        }

        internal TypeReference GetTypeReference<T>() => GetTypeReference(typeof(T));

        internal TypeReference GetTypeReference(Type type)
        {
            return moduleDefinition.ImportReference(type);
        }
    }
}
