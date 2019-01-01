using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Shared.TechEffects;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using TypeSystem = Fody.TypeSystem;

namespace Weavers.TechEffects
{
    sealed class ModifiableImplementationWeaver : BaseImplementationWeaver
    {
        public ModifiableImplementationWeaver(
                ModuleDefinition moduleDefinition,
                TypeSystem typeSystem,
                ILogger logger,
                ReferenceFinder referenceFinder)
            : base(moduleDefinition, typeSystem, logger, referenceFinder) {}

        public TypeDefinition WeaveImplementation(
            TypeReference interfaceToImplement,
            IReadOnlyCollection<PropertyDefinition> properties)
        {
            var (modifiableType, genericParameterInterface) = PrepareImplementation(
                interfaceToImplement,
                Constants.GetModifiableClassNameForInterface(interfaceToImplement.Name),
                TypeSystem.ObjectReference);
            modifiableType.BaseType =
                ReferenceFinder.GetTypeReference(Constants.ModifiableBase).MakeGenericInstanceType(modifiableType);
            
            var (fieldsByProperty, templateField) = addConstructor(modifiableType, interfaceToImplement, properties);
            
            foreach (var property in properties)
            {
                if (fieldsByProperty.ContainsKey(property))
                {
                    addFieldBackedProperty(modifiableType, property, fieldsByProperty[property]);
                    addStaticAttributeGetMethod(modifiableType, fieldsByProperty[property]);
                }
                else
                {
                    addTemplateBackedProperty(modifiableType, property, templateField);
                }
            }
            
            addStaticConstructor(modifiableType, fieldsByProperty);

            addCreateModifiableInstanceMethod(modifiableType, genericParameterInterface);
            addHasAttributeOfTypeMethod(modifiableType, genericParameterInterface);
            addModifyAttributeMethod(modifiableType, genericParameterInterface);
            addStaticAttributeIsKnownMethod(modifiableType);

            return modifiableType;
        }
        
        #region Constructor
        private (Dictionary<PropertyDefinition, FieldReference> fieldsByProperty, FieldReference templateField)
            addConstructor(
                TypeDefinition type,
                TypeReference interfaceToImplement,
                IReadOnlyCollection<PropertyDefinition> properties)
        {
            var lambdas = createLambdasObject(type, out var lambdaInstance);

            var method = new MethodDefinition(
                ".ctor",
                MethodAttributes.Public | MethodAttributes.SpecialName
                | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig,
                TypeSystem.VoidReference);
            method.Parameters.Add(
                new ParameterDefinition(
                    Constants.TemplateFieldName, ParameterAttributes.None, interfaceToImplement));

            var templateField = new FieldDefinition(
                Constants.TemplateFieldName,
                FieldAttributes.Private | FieldAttributes.InitOnly,
                interfaceToImplement);
            type.Fields.Add(templateField);
            
            var fieldsByProperty = new Dictionary<PropertyDefinition, FieldReference>();
            var fields = new List<(
                FieldReference field,
                TypeReference innerFieldType,
                MethodReference conversionLambda)>();

            var attributeWithModificationsType =
                ReferenceFinder.GetTypeReference(Constants.AttributeWithModificationsType);
            var attributeWithModificationsCtor =
                ReferenceFinder.GetConstructorReference(attributeWithModificationsType);

            foreach (var property in properties)
            {
                if (!property.TryGetCustomAttribute(typeof(ModifiableAttribute), out var modifiableAttribute)
                    || extractAttributeType(modifiableAttribute) == AttributeType.None)
                {
                    continue;
                }
                
                var fieldType = attributeWithModificationsType.MakeGenericInstanceType(property.PropertyType);

                var fieldDef = new FieldDefinition(
                    property.Name.ToCamelCase(),
                    FieldAttributes.Private | FieldAttributes.InitOnly,
                    fieldType);
                var lambdaMethod = createLambdaMethod(lambdas, fieldDef.Name, property.PropertyType);

                type.Fields.Add(fieldDef);
                fieldsByProperty.Add(property, fieldDef);
                fields.Add((
                    field: fieldDef,
                    innerFieldType: property.PropertyType,
                    conversionLambda: lambdaMethod));
            }

            var baseConstructor = ReferenceFinder.GetConstructorReference(type.BaseType);
            baseConstructor.DeclaringType = type.BaseType;

            var processor = method.Body.GetILProcessor();

            // call base constructor
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Call, baseConstructor);

            // set template field
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldarg_1);
            processor.Emit(OpCodes.Stfld, templateField);
            
            var funcCtor = ReferenceFinder.GetConstructorReference(typeof(Func<,>));
            var hasLocals = false;

            // create modifiable wrappers for all properties
            foreach (var fieldInfo in fields)
            {
                // load template on stack
                processor.Emit(OpCodes.Ldarg_0);
                processor.Emit(OpCodes.Ldarg_1);

                // get property value
                var property = ReferenceFinder
                    .GetPropertyReference(interfaceToImplement, fieldInfo.field.Name.ToTitleCase())
                    .Resolve();
                var propertyGetter = ModuleDefinition.ImportReference(property.GetMethod);
                processor.Emit(OpCodes.Callvirt, propertyGetter);

                // unwrap a nested type
                var typeOnStack = fieldInfo.innerFieldType;
                if (!typeOnStack.IsPrimitive)
                {
                    var fieldType = ModuleDefinition
                        .ImportReference(fieldInfo.innerFieldType);
                    var innerProperty = fieldType
                        .Resolve()
                        .Properties[0]
                        .Resolve();

                    if (fieldType.IsValueType)
                    {
                        var localVar = new VariableDefinition(fieldType);
                        method.Body.Variables.Add(localVar);
                        hasLocals = true;
                        // Turn the object into an address using a local variable
                        processor.Emit(OpCodes.Stloc, localVar);
                        processor.Emit(OpCodes.Ldloca, localVar);
                    }
                    
                    processor.Emit(OpCodes.Call, ModuleDefinition.ImportReference(innerProperty.GetMethod));
                    typeOnStack = innerProperty.PropertyType;
                }

                // cast to double if necessary
                if (typeOnStack.FullName != TypeSystem.DoubleReference.FullName)
                {
                    processor.Emit(OpCodes.Conv_R8);
                }

                // load lambda instance on the stack
                processor.Emit(OpCodes.Ldsfld, lambdaInstance);

                // load the function pointer on the stack
                processor.Emit(OpCodes.Ldftn, fieldInfo.conversionLambda);
                
                // create a System.Func instance of the right type
                var genericFuncCtor =
                    funcCtor.MakeHostInstanceGeneric(TypeSystem.DoubleReference, fieldInfo.innerFieldType);
                processor.Emit(OpCodes.Newobj, genericFuncCtor);

                // create the AttributeWithModifications instance
                var attributeCtor = attributeWithModificationsCtor.MakeHostInstanceGeneric(fieldInfo.innerFieldType);
                processor.Emit(OpCodes.Newobj, attributeCtor);

                // set field to this instance
                processor.Emit(OpCodes.Stfld, fieldInfo.field);
            }

            method.Body.InitLocals = hasLocals;

            processor.Emit(OpCodes.Ret);
            type.Methods.Add(method);

            return (fieldsByProperty, templateField);
        }
        #endregion

        #region Lambdas
        private TypeDefinition createLambdasObject(TypeDefinition outerType, out FieldReference instanceFieldRef)
        {
            var lambdas = new TypeDefinition(
                "",
                outerType.Name + "Lambdas",
                TypeAttributes.NestedPrivate | TypeAttributes.AutoClass | TypeAttributes.AnsiClass
                | TypeAttributes.Sealed | TypeAttributes.Serializable | TypeAttributes.BeforeFieldInit,
                TypeSystem.ObjectReference) { DeclaringType = outerType };
            outerType.NestedTypes.Add(lambdas);

            var instanceField = new FieldDefinition(
                "Instance",
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly,
                lambdas);
            lambdas.Fields.Add(instanceField);
            instanceFieldRef = instanceField;

            var ctor = MethodHelpers.AddEmptyConstructor(ReferenceFinder, TypeSystem, lambdas);
            var cctor = new MethodDefinition(
                ".cctor",
                MethodAttributes.Private | MethodAttributes.SpecialName
                | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Static,
                TypeSystem.VoidReference);

            lambdas.Methods.Add(cctor);

            var processor = cctor.Body.GetILProcessor();

            processor.Emit(OpCodes.Newobj, ctor);
            processor.Emit(OpCodes.Stsfld, instanceField);
            processor.Emit(OpCodes.Ret);

            return lambdas;
        }

        private MethodDefinition createLambdaMethod(TypeDefinition lambdas, string fieldName, TypeReference innerType)
        {
            var targetPrimitive = innerType;
            MethodReference wrapperCtor = null;

            if (!innerType.IsPrimitive)
            {
                wrapperCtor = ReferenceFinder.GetConstructorReference(innerType);
                targetPrimitive = wrapperCtor.Parameters[0].ParameterType;
            }

            var lambdaMethod = new MethodDefinition(
                fieldName + "Converter",
                MethodAttributes.Assembly | MethodAttributes.HideBySig,
                innerType);
            lambdaMethod.Parameters.Add(new ParameterDefinition(TypeSystem.DoubleReference));
            lambdas.Methods.Add(lambdaMethod);

            var processor = lambdaMethod.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_1);
            if (targetPrimitive.FullName == TypeSystem.DoubleReference.FullName)
            {
                // do nothing
            }
            else if (targetPrimitive.FullName == TypeSystem.SingleReference.FullName)
            {
                processor.Emit(OpCodes.Conv_R4);
            }
            else if (targetPrimitive.FullName == TypeSystem.Int32Reference.FullName)
            {
                processor.Emit(OpCodes.Conv_I4);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unknown conversion from double to primitive {targetPrimitive.Name}");
            }

            if (wrapperCtor != null)
            {
                processor.Emit(OpCodes.Newobj, wrapperCtor);
            }
            processor.Emit(OpCodes.Ret);
            return lambdaMethod;
        }
        #endregion
        
        #region Static constructor

        private void addStaticConstructor(
            TypeDefinition type, Dictionary<PropertyDefinition, FieldReference> fieldsByProperty)
        {
            var cctor = new MethodDefinition(
                ".cctor",
                MethodAttributes.Private | MethodAttributes.SpecialName
                | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Static,
                TypeSystem.VoidReference);
            
            var fieldsWithAttributes = getFieldsWithAttributes(fieldsByProperty);

            var processor = cctor.Body.GetILProcessor();

            var funcType = ReferenceFinder
                .GetTypeReference(typeof(Func<,>))
                .MakeGenericInstanceType(
                    type,
                    ReferenceFinder.GetTypeReference<IAttributeWithModifications>());
            var funcTypeCtor = ReferenceFinder
                .GetConstructorReference(funcType)
                .MakeHostInstanceGeneric(
                    type,
                    ReferenceFinder.GetTypeReference<IAttributeWithModifications>());
            var keyValueType = ReferenceFinder
                .GetTypeReference(typeof(KeyValuePair<,>))
                .MakeGenericInstanceType(
                    ReferenceFinder.GetTypeReference<AttributeType>(),
                    funcType);
            var keyValueCtor = ReferenceFinder
                .GetConstructorReference(typeof(KeyValuePair<,>))
                .MakeHostInstanceGeneric(
                    ReferenceFinder.GetTypeReference<AttributeType>(),
                    funcType);
            var keyValueListCtor = ReferenceFinder
                .GetConstructorReference(typeof(List<>))
                .MakeHostInstanceGeneric(keyValueType);
            var keyValueListAdd = ReferenceFinder
                .GetMethodReference(
                    ReferenceFinder.GetTypeReference(typeof(List<>)),
                    nameof(List<int>.Add))
                .MakeHostInstanceGeneric(keyValueType);
            
            // create new list
            processor.Emit(OpCodes.Newobj, keyValueListCtor);
            
            foreach (var (field, attributeType) in fieldsWithAttributes)
            {
                if (attributeType == AttributeType.None)
                {
                    throw new InvalidOperationException("Fields without attributes should be filtered out.");
                }

                // duplicate the list on stack
                processor.Emit(OpCodes.Dup);

                // load enum value as int value
                processor.Emit(OpCodes.Ldc_I4, (int) attributeType);

                // load static function
                var getAttributeMethod =
                    ReferenceFinder.GetMethodReference(type, getStaticAttributeGetMethodName(field));
                
                processor.Emit(OpCodes.Ldnull);
                processor.Emit(OpCodes.Ldftn, getAttributeMethod);
                processor.Emit(OpCodes.Newobj, funcTypeCtor);

                // create key-value pair
                processor.Emit(OpCodes.Newobj, keyValueCtor);

                // add to list
                processor.Emit(OpCodes.Callvirt, keyValueListAdd);
            }

            var methodReference =
                ReferenceFinder.GetMethodReference(type.BaseType, Constants.ModifiableBaseInitializeMethod);
            methodReference.DeclaringType = type.BaseType;
            processor.Emit(OpCodes.Call, methodReference);
            
            processor.Emit(OpCodes.Ret);
            
            type.Methods.Add(cctor);
        }

        private static List<(FieldReference, AttributeType)> getFieldsWithAttributes(
            Dictionary<PropertyDefinition, FieldReference> fieldsByProperty)
        {
            var fieldToAttributeType = new List<(FieldReference, AttributeType)>();

            foreach (var pair in fieldsByProperty)
            {
                var property = pair.Key;
                var field = pair.Value;

                if (!property.TryGetCustomAttribute(typeof(ModifiableAttribute), out var modifiableAttribute))
                {
                    continue;
                }

                var attributeType = extractAttributeType(modifiableAttribute);

                if (attributeType == AttributeType.None)
                {
                    continue;
                }

                fieldToAttributeType.Add((field, attributeType));
            }

            return fieldToAttributeType;
        }

        private static AttributeType extractAttributeType(CustomAttribute modifiableAttribute)
        {
            return (AttributeType) (modifiableAttribute
                .Properties
                .FirstOrDefault(arg => arg.Name == nameof(ModifiableAttribute.Type))
                .Argument
                .Value ?? AttributeType.None);
        }

        #endregion

        #region Properties
        private void addFieldBackedProperty(
            TypeDefinition type, PropertyDefinition propertyBase, FieldReference fieldReference)
        {
            var propertyImpl = MethodHelpers.CreatePropertyImplementation(ModuleDefinition, propertyBase);
            var getMethodImpl = propertyImpl.GetMethod;

            var fieldType = ModuleDefinition.ImportReference(fieldReference.FieldType).Resolve();
            var valueProperty = fieldType.Properties.First(p => p.Name == nameof(AttributeWithModifications<int>.Value));

            var processor = getMethodImpl.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, fieldReference);
            processor.Emit(
                OpCodes.Callvirt,
                ModuleDefinition.ImportReference(
                    valueProperty.GetMethod.MakeHostInstanceGeneric(propertyBase.PropertyType)));
            processor.Emit(OpCodes.Ret);
            type.Properties.Add(propertyImpl);
            type.Methods.Add(getMethodImpl);
        }
        
        private void addStaticAttributeGetMethod(TypeDefinition type, FieldReference attributeField)
        {
            var method = new MethodDefinition(
                getStaticAttributeGetMethodName(attributeField),
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static,
                ReferenceFinder.GetTypeReference<IAttributeWithModifications>());
            method.Parameters.Add(new ParameterDefinition("instance", ParameterAttributes.None, type));

            var processor = method.Body.GetILProcessor();
            
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, attributeField);
            processor.Emit(OpCodes.Ret);
            
            type.Methods.Add(method);
        }

        private static string getStaticAttributeGetMethodName(MemberReference attributeField)
        {
            return $"get{attributeField.Name.ToTitleCase()}";
        }

        private void addTemplateBackedProperty(
            TypeDefinition type, PropertyDefinition property, FieldReference templateField)
        {
            var propertyImpl = MethodHelpers.CreatePropertyImplementation(ModuleDefinition, property);
            var getMethodImpl = propertyImpl.GetMethod;
            
            var templateProperty = ModuleDefinition.ImportReference(templateField.FieldType)
                .Resolve()
                .Properties
                .First(p => p.Name == property.Name);

            var processor = getMethodImpl.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, templateField);
            processor.Emit(OpCodes.Callvirt, ModuleDefinition.ImportReference(templateProperty.GetMethod));
            processor.Emit(OpCodes.Ret);
            type.Properties.Add(propertyImpl);
            type.Methods.Add(getMethodImpl);
        }
        #endregion

        #region Simple method implementations
        private void addCreateModifiableInstanceMethod(
            TypeDefinition type, GenericInstanceType genericParameterInterface)
        {
            var field = type.Fields.First(f => f.Name == Constants.TemplateFieldName);
            ImplementCreateModifiableInstanceMethod(type, genericParameterInterface, type, field);
        }

        private void addHasAttributeOfTypeMethod(
            TypeDefinition type, TypeReference interfaceType)
        {
            var baseMethod = ReferenceFinder
                .GetMethodReference(type.BaseType, Constants.HasAttributeOfTypeMethod)
                .MakeHostInstanceGeneric(type);
            
            AddVirtualMethodImplementation(interfaceType, type, baseMethod);
        }

        private void addModifyAttributeMethod(TypeDefinition type, TypeReference interfaceType)
        {
            var baseMethod = ReferenceFinder
                .GetMethodReference(type.BaseType, Constants.ModifyAttributeMethod)
                .MakeHostInstanceGeneric(type);
            
            AddVirtualMethodImplementation(interfaceType, type, baseMethod);
        }

        private void addStaticAttributeIsKnownMethod(TypeDefinition type)
        {
            var templateMethod = ReferenceFinder
                .GetMethodReference(type.BaseType, Constants.AttributeIsKnownMethod)
                .MakeHostInstanceGeneric(type);

            var method = new MethodDefinition(
                Constants.AttributeIsKnownMethod,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Static,
                TypeSystem.BooleanReference);
            method.Parameters.Add(new ParameterDefinition("type", ParameterAttributes.None,
                ReferenceFinder.GetTypeReference<AttributeType>()));

            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Call, templateMethod);
            processor.Emit(OpCodes.Ret);

            type.Methods.Add(method);
        }

        #endregion
    }
}
