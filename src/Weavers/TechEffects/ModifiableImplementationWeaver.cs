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
        private readonly TypeReference baseClassType;

        public ModifiableImplementationWeaver(
                ModuleDefinition moduleDefinition,
                TypeSystem typeSystem,
                ILogger logger,
                ReferenceFinder referenceFinder)
            : base(moduleDefinition, typeSystem, logger, referenceFinder)
        {
            baseClassType = ReferenceFinder.GetTypeReference(Constants.ModifiableBase);
        }

        public TypeDefinition WeaveImplementation(
            TypeReference interfaceToImplement,
            IReadOnlyCollection<PropertyDefinition> properties)
        {
            var (modifiableType, genericParameterInterface) = PrepareImplementation(
                interfaceToImplement,
                Constants.GetModifiableClassNameForInterface(interfaceToImplement.Name),
                baseClassType);

            var (fieldsByProperty, templateField) = addConstructor(modifiableType, interfaceToImplement, properties);

            foreach (var property in properties)
            {
                if (fieldsByProperty.ContainsKey(property))
                {
                    addFieldBackedProperty(modifiableType, property, fieldsByProperty[property]);
                }
                else
                {
                    addTemplateBackedProperty(modifiableType, property, templateField);
                }
            }

            addCreateModifiableInstanceMethod(modifiableType, genericParameterInterface);
            addModifyAttributeMethod(interfaceToImplement, modifiableType);

            return modifiableType;
        }
        
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
                AttributeType attributeType,
                TypeReference innerFieldType,
                MethodReference conversionLambda)>();

            var attributeWithModificationsType =
                ReferenceFinder.GetTypeReference(Constants.AttributeWithModificationsType);
            var attributeWithModificationsCtor =
                ReferenceFinder.GetConstructorReference(attributeWithModificationsType);

            foreach (var property in properties)
            {
                property.TryGetCustomAttribute(typeof(ModifiableAttribute), out var modifiableAttribute);

                var attributeType = (AttributeType) (modifiableAttribute
                    .Properties
                    .FirstOrDefault(arg => arg.Name == nameof(ModifiableAttribute.Type))
                    .Argument
                    .Value ?? AttributeType.None);

                if (attributeType == AttributeType.None)
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
                    attributeType: attributeType,
                    innerFieldType: property.PropertyType,
                    conversionLambda: lambdaMethod));
            }

            var baseConstructor = ReferenceFinder.GetConstructorReference(baseClassType);

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

                    var localVar = new VariableDefinition(fieldType);
                    method.Body.Variables.Add(localVar);
                    hasLocals = true;
                    // Turn the object into an address using a local variable
                    processor.Emit(OpCodes.Stloc, localVar);
                    processor.Emit(OpCodes.Ldloca, localVar);
                    
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

            // prepare a 'this' on the stack for later
            processor.Emit(OpCodes.Ldarg_0);

            var keyValueType = ReferenceFinder
                .GetTypeReference(typeof(KeyValuePair<,>))
                .MakeGenericInstanceType(
                    ReferenceFinder.GetTypeReference<AttributeType>(),
                    ReferenceFinder.GetTypeReference<IAttributeWithModifications>());
            var keyValueCtor = ReferenceFinder
                .GetConstructorReference(typeof(KeyValuePair<,>))
                .MakeHostInstanceGeneric(
                    ReferenceFinder.GetTypeReference<AttributeType>(),
                    ReferenceFinder.GetTypeReference<IAttributeWithModifications>());
            var keyValueListCtor = ReferenceFinder
                .GetConstructorReference(typeof(List<>))
                .MakeHostInstanceGeneric(keyValueType);
            var keyValueListAdd = ReferenceFinder
                .GetMethodReference<List<KeyValuePair<AttributeType, IAttributeWithModifications>>>(
                    l => l.Add(new KeyValuePair<AttributeType, IAttributeWithModifications>()));

            // create list of key-value pairs of attribute type and attribute where modifiable
            processor.Emit(OpCodes.Newobj, keyValueListCtor);
            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.attributeType == AttributeType.None) continue;

                // duplicate the list on stack
                processor.Emit(OpCodes.Dup);

                // load enum value as int value
                processor.Emit(OpCodes.Ldc_I4, (int) fieldInfo.attributeType);

                // load field object
                processor.Emit(OpCodes.Ldarg_0);
                processor.Emit(OpCodes.Ldfld, fieldInfo.field);

                // create key-value pair
                processor.Emit(OpCodes.Newobj, keyValueCtor);

                // add to list
                processor.Emit(OpCodes.Callvirt, keyValueListAdd);
            }

            // call base dictionary method
            var methodReference =
                ReferenceFinder.GetMethodReference(baseClassType, Constants.ModifiableBaseInitializeMethod);
            processor.Emit(OpCodes.Call, methodReference);

            processor.Emit(OpCodes.Ret);
            type.Methods.Add(method);

            return (fieldsByProperty, templateField);
        }

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

            //lambdas.Methods.Add(ctor);
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

        private void addCreateModifiableInstanceMethod(TypeDefinition type, GenericInstanceType genericParameterInterface)
        {
            var field = type.Fields.First(f => f.Name == Constants.TemplateFieldName);
            ImplementCreateModifiableInstanceMethod(type, genericParameterInterface, type, field);
        }

        private void addModifyAttributeMethod(TypeReference interfaceToImplement, TypeDefinition type)
        {
            var baseMethod = ReferenceFinder
                .GetMethodReference<ModifiableBase>(b =>
                    b.ModifyAttribute(AttributeType.None, new Modification()))
                .Resolve();

            var method = new MethodDefinition(
                baseMethod.Name,
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig
                | MethodAttributes.ReuseSlot | MethodAttributes.Virtual,
                baseMethod.ReturnType);
            foreach (var p in baseMethod.Parameters)
            {
                method.Parameters.Add(new ParameterDefinition(
                    p.Name, p.Attributes, ModuleDefinition.ImportReference(p.ParameterType)));
            }

            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            for (var i = 1; i <= baseMethod.Parameters.Count; i++)
            {
                processor.Emit(OpCodes.Ldarg, i);
            }
            processor.Emit(OpCodes.Call, ModuleDefinition.ImportReference(baseMethod));
            processor.Emit(OpCodes.Ret);
            
            var interfaceMethod = ReferenceFinder
                .GetMethodReference(
                    ModuleDefinition.ImportReference(Constants.Interface),
                    baseMethod.Name)
                .MakeHostInstanceGeneric(interfaceToImplement);
            method.Overrides.Add(ModuleDefinition.ImportReference(interfaceMethod));
            type.Methods.Add(method);
        }
    }
}
