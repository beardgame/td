using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
// ReSharper disable BuiltInTypeReferenceStyle

namespace Weavers
{
    static class ILHelpers
    {
        public static void EmitLd(ILProcessor ilProcessor, TypeReference typeReference, object value)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (typeReference.MetadataType)
            {
                // Basic number types
                case MetadataType.SByte:
                case MetadataType.Boolean:
                    ilProcessor.Emit(OpCodes.Ldc_I4_S, Convert.ToSByte(value));
                    break;

                case MetadataType.Char:
                    ilProcessor.Emit(OpCodes.Ldc_I4, Convert.ToChar(value));
                    break;

                case MetadataType.Byte:
                    ilProcessor.Emit(OpCodes.Ldc_I4, Convert.ToInt32(value));
                    break;

                // Integer types
                case MetadataType.Int16:
                    ilProcessor.Emit(OpCodes.Ldc_I4, Convert.ToInt16(value));
                    break;

                case MetadataType.UInt16:
                    unchecked
                    {
                        ilProcessor.Emit(OpCodes.Ldc_I4, (Int16) (UInt16) value);
                    }
                    break;

                case MetadataType.Int32:
                    ilProcessor.Emit(OpCodes.Ldc_I4, Convert.ToInt32(value));
                    break;

                case MetadataType.UInt32:
                    unchecked
                    {
                        ilProcessor.Emit(OpCodes.Ldc_I4, (Int32) (UInt32) value);
                    }
                    break;

                case MetadataType.Int64:
                    ilProcessor.Emit(OpCodes.Ldc_I8, Convert.ToInt64(value));
                    break;

                case MetadataType.UInt64:
                    unchecked
                    {
                        ilProcessor.Emit(OpCodes.Ldc_I8, (Int64) (UInt64) value);
                    }
                    break;

                // Floating point types
                case MetadataType.Single:
                    ilProcessor.Emit(OpCodes.Ldc_R4, Convert.ToSingle(value));
                    break;

                case MetadataType.Double:
                    ilProcessor.Emit(OpCodes.Ldc_R8, Convert.ToDouble(value));
                    break;

                // String
                case MetadataType.String:
                    ilProcessor.Emit(OpCodes.Ldstr, (string) value);
                    break;
                
                default:
                    throw new ArgumentException("Unsupported attribute parameter type.", nameof(typeReference));
            }
        }
    }
}
