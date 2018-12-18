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
            // Note: this PrepareImplementation and the one in the template implementation weaver will have to be
            // pulled out since the weavers have to refer to each other. Prepare implementation first (which will create
            // the type and set up the basic type metadata), then pass the type definitions into the weave functions.
            var (modifiableType, genericParameterInterface) = PrepareImplementation(
                interfaceToImplement,
                Constants.GetModifiableClassNameForInterface(interfaceToImplement.Name),
                baseClassType);
            modifiableType.BaseType = baseClassType.MakeGenericInstanceType(modifiableType);
            
            var (fieldsByProperty, templateField) = addConstructor(modifiableType, interfaceToImplement, properties);
            
            foreach (var property in properties)
            {
                if (fieldsByProperty.ContainsKey(property))
                {
                    // Note: field backed properties are the modifiable ones, so this might be a good place to also add
                    // the static functions that you will use in the static constructor.
                    addFieldBackedProperty(modifiableType, property, fieldsByProperty[property]);
                }
                else
                {
                    addTemplateBackedProperty(modifiableType, property, templateField);
                }
            }
            
            // Note: This is probably the place where you will be wanting to add the static constructor.
            // Static constructors have the special name .cctor. You can see how the lambdas object works for an idea
            // of how to define a static constructor. I have marked some code in the constructor code below to explain
            // how you can get the modifiable types. My suggestion is to extract that code and generate a map from
            // property to attribute type that you can reuse in addConstructor.

            addCreateModifiableInstanceMethod(modifiableType, genericParameterInterface);
            addHasAttributeOfTypeMethod(modifiableType, genericParameterInterface);
            addModifyAttributeMethod(modifiableType, genericParameterInterface);

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
                if (!property.TryGetCustomAttribute(typeof(ModifiableAttribute), out var modifiableAttribute))
                {
                    continue;
                }

                // Note: this bit of ugly code looks through all the properties in the type of the Modifiable attribute
                // and gets the value of it. This will be the AttributeType for this property.
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
            
            // Note: everything starting here is used for the current InitializeAttributes. That means you can probably
            // kill most of this, but I am leaving it here for inspiration, since this took many hours of hard work to
            // get running.

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
            
            // Note: when you delete the old code, you will want to leave the lines below.

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

        private void addCreateModifiableInstanceMethod(
            TypeDefinition type, GenericInstanceType genericParameterInterface)
        {
            var field = type.Fields.First(f => f.Name == Constants.TemplateFieldName);
            ImplementCreateModifiableInstanceMethod(type, genericParameterInterface, type, field);
        }

        // Note: apparently this method doesn't properly do the virtual method implementation (same with the next method)
        // Probably something to do with the generic instance, but the IL looks as I expect.
        // Maybe the generic parameter of the base class gets lost somewhere in AddVirtualMethodImplementation
        private void addHasAttributeOfTypeMethod(
            TypeDefinition type, TypeReference genericParameterInterface)
        {
            var baseMethod = ReferenceFinder
                .GetMethodReference(type.BaseType, Constants.HasAttributeOfTypeMethod)
                .MakeHostInstanceGeneric(type);
            
            AddVirtualMethodImplementation(genericParameterInterface, type, baseMethod);
        }

        private void addModifyAttributeMethod(TypeDefinition type, TypeReference genericParameterInterface)
        {
            var baseMethod = ReferenceFinder
                .GetMethodReference(type.BaseType, Constants.ModifyAttributeMethod)
                .MakeHostInstanceGeneric(type);
            
            AddVirtualMethodImplementation(genericParameterInterface, type, baseMethod);
        }
    }
}
