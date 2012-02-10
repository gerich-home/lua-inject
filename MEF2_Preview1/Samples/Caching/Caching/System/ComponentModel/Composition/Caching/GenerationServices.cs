// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Internal;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition.Caching
{
    internal class GenerationServices
    {
        // Type.GetTypeFromHandle
        private static readonly MethodInfo _typeGetTypeFromHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle");


        // typeofs are pretty expensive, so we cache them statically
        private static readonly Type TypeType = typeof(System.Type);
        private static readonly Type StringType = typeof(System.String);
        private static readonly Type CharType = typeof(System.Char);
        private static readonly Type BooleanType = typeof(System.Boolean);
        private static readonly Type ByteType = typeof(System.Byte);
        private static readonly Type SByteType = typeof(System.SByte);
        private static readonly Type Int16Type = typeof(System.Int16);
        private static readonly Type UInt16Type = typeof(System.UInt16);
        private static readonly Type Int32Type = typeof(System.Int32);
        private static readonly Type UInt32Type = typeof(System.UInt32);
        private static readonly Type Int64Type = typeof(System.Int64);
        private static readonly Type UInt64Type = typeof(System.UInt64);
        private static readonly Type DoubleType = typeof(System.Double);
        private static readonly Type SingleType = typeof(System.Single);
        private static readonly Type ObjectType = typeof(System.Object);
        private static readonly Type GenericIDictionaryType = typeof(System.Collections.Generic.IDictionary<,>);
        private static readonly Type GenericDictionaryType = typeof(System.Collections.Generic.Dictionary<,>);
        private static readonly Type IEnumerableTypeofT = typeof(System.Collections.Generic.IEnumerable<>);
        private static readonly Type IEnumerableType = typeof(System.Collections.IEnumerable);

        private static readonly Type standardDictionaryType = typeof(System.Collections.Generic.Dictionary<string, object>);
        private static readonly MethodInfo standardDictionaryAddMethod = standardDictionaryType.GetMethod("Add", new Type[] { StringType, ObjectType });
        private static readonly ConstructorInfo standardDictionaryConstructor = standardDictionaryType.GetConstructor(new Type[] { Int32Type });

        private const string StandardDictionaryGeneratorStubName = "StandardDictionaryGeneratorStub";

        private const ushort StandardDictionaryGeneratorsCount = 10;

        private MethodInfo[] _standardDictionaryGenerators;

        public GenerationServices(ModuleBuilder moduleBuilder)
        {
            Assumes.NotNull(moduleBuilder);
            this._standardDictionaryGenerators = GenerationServices.GetStandardDictionaryGenerators(moduleBuilder);
        }

        /// Generates the code that loads the supplied value on the stack
        /// This is not as simple as it seems, as different instructions need to be generated depending
        /// on its type.
        /// We support:
        /// 1. All primitive types and IntPtrs
        /// 2. Strings
        /// 3. Enums
        /// 4. typeofs
        /// 5. nulls
        /// 6. Dictionaries of (string, object) recursively containing all of the above
        /// 7. Enumerables
        /// Everything else cannot be represented as literals
        /// <param name="ilGenerator"></param>
        /// <param name="item"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal CachingResult LoadValue(ILGenerator ilGenerator, object value)
        {
            Assumes.NotNull(ilGenerator);
            CachingResult result = CachingResult.SucceededResult;

            //
            // Get nulls out of the way - they are basically typeless, so we just load null
            //
            if (value == null)
            {
                return GenerationServices.LoadNull(ilGenerator);
            }

            //
            // Prepare for literal loading - decide whether we should box, and handle enums properly
            //
            Type valueType = value.GetType();
            object rawValue = value;
            if (valueType.IsEnum)
            {
                // enums are special - we need to load the underlying constant on the stack
                rawValue = Convert.ChangeType(value, Enum.GetUnderlyingType(valueType), null);
                valueType = rawValue.GetType();
            }

            //
            // Generate IL depending on the valueType - this is messier than it should ever be, but sadly necessary
            //
            Type dictionaryKeyType;
            Type dictionaryValueType;
            IDictionary<string, object> standardDictionary = value as IDictionary<string, object>;
            if (standardDictionary != null)
            {
                if (standardDictionary.Count < GenerationServices.StandardDictionaryGeneratorsCount)
                {
                    return this.LoadStandardDictionaryFast(ilGenerator, standardDictionary);
                }
                else
                {
                    return this.LoadGenericDictionary(ilGenerator, standardDictionary, true);
                }
            }
            else if (GenerationServices.TryGetDictionaryElementType(valueType, out dictionaryKeyType, out dictionaryValueType))
            {
                result = result.MergeResult(this.LoadDictionary(ilGenerator, rawValue, dictionaryKeyType, dictionaryValueType));
            }
            else if (valueType == GenerationServices.StringType)
            {
                // we need to check for strings before enumerables, because strings are IEnumerable<char>
                result = result.MergeResult(GenerationServices.LoadString(ilGenerator,(string)rawValue));
            }
            else if (GenerationServices.TypeType.IsAssignableFrom(valueType))
            {
                result = result.MergeResult(GenerationServices.LoadTypeOf(ilGenerator, (Type)rawValue));
            }
            else if (GenerationServices.IEnumerableType.IsAssignableFrom(valueType))
            {
                // NOTE : strings and dictionaries are also enumerables, but we have already handled those
                result = result.MergeResult(this.LoadEnumerable(ilGenerator, (IEnumerable) rawValue));
            }
            else if (
                (valueType == GenerationServices.CharType) ||
                (valueType == GenerationServices.BooleanType) ||
                (valueType == GenerationServices.ByteType) ||
                (valueType == GenerationServices.SByteType) ||
                (valueType == GenerationServices.Int16Type) ||
                (valueType == GenerationServices.UInt16Type) ||
                (valueType == GenerationServices.Int32Type)
                )
            {
                // NOTE : Everything that is 32 bit or less uses ldc.i4. We need to pass int32, even if the actual types is shorter - this is IL memory model
                // direct casting to (int) won't work, because the value is boxed, thus we need to use Convert.
                // Sadly, this will not work for all cases - namely large uint32 - because they can't semantically fit into 32 signed bits
                // We have a special case for that next
                result = result.MergeResult(GenerationServices.LoadInt(ilGenerator, (int)Convert.ChangeType(rawValue, typeof(int), CultureInfo.InvariantCulture)));
            }
            else if (valueType == GenerationServices.UInt32Type)
            {
                // NOTE : This one is a bit tricky. Ldc.I4 takes an Int32 as an argument, although it really treats it as a 32bit number
                // That said, some UInt32 values are larger that Int32.MaxValue, so the Convert call above will fail, which is why 
                // we need to treat this case individually and cast to uint, and then - unchecked - to int.
                result = result.MergeResult(GenerationServices.LoadInt(ilGenerator, unchecked((int)((uint)rawValue))));
            }
            else if (valueType == GenerationServices.Int64Type)
            {
                result = result.MergeResult(GenerationServices.LoadLong(ilGenerator, (long)rawValue));
            }
            else if (valueType == GenerationServices.UInt64Type)
            {
                // NOTE : This one is a bit tricky. Ldc.I8 takes an Int64 as an argument, although it really treats it as a 64bit number
                // That said, some UInt64 values are larger that Int64.MaxValue, so the direct case we use above (or Convert, for that matter)will fail, which is why
                // we need to treat this case individually and cast to ulong, and then - unchecked - to long.
                result = result.MergeResult(GenerationServices.LoadLong(ilGenerator, unchecked((long)((ulong)rawValue))));
            }
            else if (valueType == GenerationServices.SingleType)
            {
                result = result.MergeResult(GenerationServices.LoadFloat(ilGenerator, (float)rawValue));
            }
            else if (valueType == GenerationServices.DoubleType)
            {
                result = result.MergeResult(GenerationServices.LoadDouble(ilGenerator, (double)rawValue));
            }
            else
            {
                result = result.MergeError(Strings.UnsupportedCacheValue, value.GetType().FullName);

                // Make sure the IL is balanced - generate the ldnull instead
                GenerationServices.LoadNull(ilGenerator);
            }

            return result;
        }

        private CachingResult LoadDictionary(ILGenerator ilGenerator, object dictionary, Type keyType, Type valueType)
        {
            Assumes.NotNull(ilGenerator);
            Assumes.NotNull(dictionary);
            Assumes.NotNull(keyType);
            Assumes.NotNull(valueType);

            return (CachingResult)(typeof(GenerationServices).GetMethod("LoadGenericDictionary", BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(keyType, valueType)
                .Invoke(this, new object[] { ilGenerator, dictionary, false }));
        }

        private CachingResult LoadGenericDictionary<TKey, TValue>(ILGenerator ilGenerator, IDictionary<TKey, TValue> dictionary, bool isStandardDictionary)
        {
            Assumes.NotNull(ilGenerator);
            Assumes.NotNull(dictionary);

            CachingResult result = CachingResult.SucceededResult;
            Type keyType = GenerationServices.NormalizeCollectionElementType(typeof(TKey));
            Type valueType = GenerationServices.NormalizeCollectionElementType(typeof(TValue));

            Type dictionaryType = null;
            MethodInfo dictionaryAddMethod = null;
            ConstructorInfo dictionaryConstructor = null;
            if (isStandardDictionary)
            {
                dictionaryType = GenerationServices.standardDictionaryType;
                dictionaryAddMethod = GenerationServices.standardDictionaryAddMethod;
                dictionaryConstructor = dictionaryType.GetConstructor(new Type[] { Int32Type });
            }
            else
            {
                dictionaryType = GenerationServices.GenericDictionaryType.MakeGenericType(keyType, valueType);
                dictionaryAddMethod = dictionaryType.GetMethod("Add", new Type[] { keyType, valueType });
                dictionaryConstructor = dictionaryType.GetConstructor(new Type[] { Int32Type });
            }

            //
            // Dictionary<TKey, TValue> metadata = new Dictionary<TKey, TValue>(capacity)
            //

            // create and load the dictionary
            GenerationServices.LoadInt(ilGenerator, dictionary.Count);
            ilGenerator.Emit(OpCodes.Newobj, dictionaryConstructor);


            // 
            // Generate a sequence of "Add" statements
            //
            
            foreach (KeyValuePair<TKey, TValue> dictionaryItem in dictionary)
            {
                //
                // metadata.Add(key, value)
                //

                // the dictionary is on top of the stack - load it again
                ilGenerator.Emit(OpCodes.Dup);
               

                // load the key, boxing if necessary
                result = result.MergeResult(this.LoadValue(ilGenerator, dictionaryItem.Key));
                // key = string for standard dictionaries, so no boxing is ever required
                if (!isStandardDictionary && GenerationServices.IsBoxingRequiredForValue(dictionaryItem.Key) && !keyType.IsValueType)
                {
                    ilGenerator.Emit(OpCodes.Box, dictionaryItem.Key.GetType());
                }

                // load the value, boxing if necessary
                result = result.MergeResult(this.LoadValue(ilGenerator, dictionaryItem.Value));
                // key = object for standard dictionaries, so value type is never a struct
                if (GenerationServices.IsBoxingRequiredForValue(dictionaryItem.Value) && (isStandardDictionary || !valueType.IsValueType) )
                {
                    ilGenerator.Emit(OpCodes.Box, dictionaryItem.Value.GetType());
                }

                // Caal the "Add"
                ilGenerator.EmitCall(OpCodes.Call, dictionaryAddMethod, null);

                // At this point the dictionary, key and value have been popped off the stack, and we ended up with the origical state of the dictionary on top of the stack
            }

            // 
            // the dicationary is already loaded on the stack - exit
            //
            return result;

        }

        private CachingResult LoadStandardDictionaryFast(ILGenerator ilGenerator, IDictionary<string, object> dictionary)
        {
            Assumes.NotNull(ilGenerator);
            Assumes.NotNull(dictionary);
            Assumes.IsTrue(dictionary.Count < GenerationServices.StandardDictionaryGeneratorsCount);

            CachingResult result = CachingResult.SucceededResult;
            MethodInfo standardDictionaryGenerator = this._standardDictionaryGenerators[dictionary.Count];

            // all we need to do is load all keys and values on stack and then invoke the standard generator
            foreach (KeyValuePair<string, object> dictionaryItem in dictionary)
            {
                // load key - boxing is never required for strings
                result = result.MergeResult(this.LoadValue(ilGenerator, dictionaryItem.Key));
                // load value
                result = result.MergeResult(this.LoadValue(ilGenerator, dictionaryItem.Value));
                if (GenerationServices.IsBoxingRequiredForValue(dictionaryItem.Value))
                {
                    ilGenerator.Emit(OpCodes.Box, dictionaryItem.Value.GetType());
                }
            }

            // call the standard dictionary generator - this would load the value on stack
            ilGenerator.EmitCall(OpCodes.Call, standardDictionaryGenerator, null);

            return result;
        }


        private CachingResult LoadEnumerable(ILGenerator ilGenerator, IEnumerable enumerable)
        {
            Assumes.NotNull(ilGenerator);
            Assumes.NotNull(enumerable);

            CachingResult result = CachingResult.SucceededResult;

            // We load enumerable as an array - this is the most compact and efficient way of representing it
            Type elementType = null;
            Type closedType = null;
            if (GenerationServices.TryGetGenericInterfaceType(enumerable.GetType(), GenerationServices.IEnumerableTypeofT, out closedType))
            {
                elementType = closedType.GetGenericArguments()[0];
            }
            else
            {
                elementType = typeof(object);
            }
            elementType = GenerationServices.NormalizeCollectionElementType(elementType);

            //
            // elem[] array = new elem[<enumerable.Count()>]
            //
            GenerationServices.LoadInt(ilGenerator, enumerable.Cast<object>().Count());
            ilGenerator.Emit(OpCodes.Newarr, elementType);
            // at this point we have the array on the stack

            int index = 0;
            foreach (object value in enumerable)
            {
                //
                //array[<index>] = value;
                //
                // load the array on teh stack again
                ilGenerator.Emit(OpCodes.Dup);
                GenerationServices.LoadInt(ilGenerator, index);
                result = result.MergeResult(this.LoadValue(ilGenerator, value));
                if (GenerationServices.IsBoxingRequiredForValue(value) && !elementType.IsValueType)
                {
                    ilGenerator.Emit(OpCodes.Box, value.GetType());
                }
                ilGenerator.Emit(OpCodes.Stelem, elementType);
                index++;
                // at this point we have the array on teh stack again
            }

            // the array is already on the stack - just exit

            return result;
        }


        internal static bool IsLoadable(object value)
        {
            Type valueType = (value != null) ? value.GetType() : null;
            return (
                (valueType == null) ||
                (valueType.IsPrimitive) ||
                valueType.IsEnum ||
                (value is string) ||
                (value is Type)||
                GenerationServices.IsLoadableDictionaryType(valueType) ||
                (value is IEnumerable) 
                );
        }

        private static Type NormalizeCollectionElementType(Type type)
        {
            if (GenerationServices.IEnumerableType.IsAssignableFrom(type) && type != GenerationServices.StringType)
            {
                // the element is IEnumerable. we need to normalize it to be literally IEnumerable
                Type closedType = null;
                if (GenerationServices.TryGetGenericInterfaceType(type, GenerationServices.IEnumerableTypeofT, out closedType))
                {
                    return GenerationServices.IEnumerableTypeofT.MakeGenericType(GenerationServices.NormalizeCollectionElementType(closedType.GetGenericArguments()[0]));
                }
                else
                {
                    return typeof(IEnumerable<object>);
                }
            }
            else
            {
                return type;
            }
        }


        private static bool IsLoadableDictionaryType(Type type)
        {
            Assumes.NotNull(type);
            Type keyType = null;
            Type valueType = null;

            return GenerationServices.TryGetDictionaryElementType(type, out keyType, out valueType);
        }

        private static bool TryGetDictionaryElementType(Type type, out Type keyType, out Type valueType)
        {
            Assumes.NotNull(type);
            keyType = null;
            valueType = null;

            if (!type.IsGenericType)
            {
                return false;
            }

            Type genericType = type.GetGenericTypeDefinition();
            Assumes.NotNull(genericType);

            Type closedInterfaceType = type.GetInterface(GenerationServices.GenericIDictionaryType.Name, false);
            if (closedInterfaceType == null)
            {
                return false;
            }

            keyType = closedInterfaceType.GetGenericArguments()[0];
            valueType = closedInterfaceType.GetGenericArguments()[1];

            return GenerationServices.IsLoadable(keyType) && GenerationServices.IsLoadable(valueType);
        }

        private static bool IsBoxingRequiredForValue(object value)
        {
            if (value == null)
            {
                return false;
            }
            else
            {
                return value.GetType().IsValueType;
            }
        }


        private static CachingResult LoadNull(ILGenerator ilGenerator)
        {
            ilGenerator.Emit(OpCodes.Ldnull);
            return CachingResult.SucceededResult;
        }

        private static CachingResult LoadString(ILGenerator ilGenerator, string s)
        {
            Assumes.NotNull(ilGenerator);
            if (s == null)
            {
                return GenerationServices.LoadNull(ilGenerator);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldstr, s);
            }
            return CachingResult.SucceededResult;
        }


        private static CachingResult LoadInt(ILGenerator ilGenerator, int value)
        {
            Assumes.NotNull(ilGenerator);
            ilGenerator.Emit(OpCodes.Ldc_I4, value);
            return CachingResult.SucceededResult;
        }

        private static CachingResult LoadLong(ILGenerator ilGenerator, long value)
        {
            Assumes.NotNull(ilGenerator);
            ilGenerator.Emit(OpCodes.Ldc_I8, value);
            return CachingResult.SucceededResult;
        }

        private static CachingResult LoadFloat(ILGenerator ilGenerator, float value)
        {
            Assumes.NotNull(ilGenerator);
            ilGenerator.Emit(OpCodes.Ldc_R4, value);
            return CachingResult.SucceededResult;
        }

        private static CachingResult LoadDouble(ILGenerator ilGenerator, double value)
        {
            Assumes.NotNull(ilGenerator);
            ilGenerator.Emit(OpCodes.Ldc_R8, value);
            return CachingResult.SucceededResult;
        }

        private static CachingResult LoadTypeOf(ILGenerator ilGenerator, Type type)
        {
            Assumes.NotNull(ilGenerator);
            //typeofs() translate into ldtoken and Type::GetTypeFromHandle call
            ilGenerator.Emit(OpCodes.Ldtoken, type);
            ilGenerator.EmitCall(OpCodes.Call, GenerationServices._typeGetTypeFromHandleMethod, null);
            return CachingResult.SucceededResult;
        }

        
        internal static bool TryGetGenericInterfaceType(Type instanceType, Type targetOpenInterfaceType, out Type targetClosedInterfaceType)
        {
            // The interface must be open
            Assumes.IsTrue(targetOpenInterfaceType.IsInterface && targetOpenInterfaceType.IsGenericTypeDefinition);
            Assumes.IsTrue(!instanceType.IsGenericTypeDefinition);

            // if instanceType is an interface, we must first check it directly
            if (instanceType.IsInterface &&
                instanceType.IsGenericType &&
                instanceType.GetGenericTypeDefinition() == targetOpenInterfaceType)
            {
                targetClosedInterfaceType = instanceType;
                return true;
            }

            try
            {
                // Purposefully not using FullName here because it results in a significantly
                //  more expensive implementation of GetInterface, this does mean that we're
                //  takign the chance that there aren't too many types which implement multiple
                //  interfaces by the same name...
                Type targetInterface = instanceType.GetInterface(targetOpenInterfaceType.Name, false);
                if (targetInterface != null &&
                    targetInterface.GetGenericTypeDefinition() == targetOpenInterfaceType)
                {
                    targetClosedInterfaceType = targetInterface;
                    return true;
                }
            }
            catch (AmbiguousMatchException)
            {
                // On the off chance that the type has >1 interfaces that it implements 
                //  with the same name, we can use this to disambiguate... 
                //
                // However, maybe this isn't great because it means that we can end up with 
                //  situations where we implement the same interface over two different types 
                //  and will always hit the slow path through an exception... 

                foreach (Type type in instanceType.GetInterfaces())
                {
                    if (type.IsGenericType &&
                        type.GetGenericTypeDefinition() == targetOpenInterfaceType)
                    {
                        targetClosedInterfaceType = type;
                        return true;
                    }
                }
            }

            targetClosedInterfaceType = null;
            return false;
        }

        private static MethodInfo[] GetStandardDictionaryGenerators(ModuleBuilder moduleBuilder)
        {
            Assumes.NotNull(moduleBuilder);

            Type standardDictionaryGeneratorStubType = moduleBuilder.GetType(GenerationServices.StandardDictionaryGeneratorStubName);
            if (standardDictionaryGeneratorStubType == null)
            {
                return GenerationServices.CreateStandardDictionaryGenerators(moduleBuilder);
            }
            else
            {
                MethodInfo[] standardDictionaryGenerators = new MethodInfo[GenerationServices.StandardDictionaryGeneratorsCount];
                MethodInfo[] standardDictionaryGeneratorsUnordered = standardDictionaryGeneratorStubType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
                Assumes.IsTrue(standardDictionaryGeneratorsUnordered.Length == GenerationServices.StandardDictionaryGeneratorsCount);
                foreach (MethodInfo standardDictionaryGenerator in standardDictionaryGeneratorsUnordered)
                {
                    int capacity = standardDictionaryGenerator.GetParameters().Length / 2;
                    standardDictionaryGenerators[capacity] = standardDictionaryGenerator;
                }
                return standardDictionaryGenerators;
            }
        }


        private static MethodInfo[] CreateStandardDictionaryGenerators(ModuleBuilder moduleBuilder)
        {
            Assumes.NotNull(moduleBuilder);
            TypeBuilder standardDictionaryGeneratorStubTypeBuilder = moduleBuilder.DefineType(
                GenerationServices.StandardDictionaryGeneratorStubName,
                TypeAttributes.NotPublic | TypeAttributes.Abstract);

            MethodInfo[] standardDictionaryGenerators = new MethodInfo[GenerationServices.StandardDictionaryGeneratorsCount];
            for (ushort i = 0; i < GenerationServices.StandardDictionaryGeneratorsCount; i++)
            {
                standardDictionaryGenerators[i] = GenerationServices.CreateStandardDictionaryGenerator(standardDictionaryGeneratorStubTypeBuilder, i);
            }

            standardDictionaryGeneratorStubTypeBuilder.CreateType();
            return standardDictionaryGenerators;
        }

        private static MethodInfo CreateStandardDictionaryGenerator(TypeBuilder standardDictionaryGeneratorStubTypeBuilder, ushort capacity)
        {
            Assumes.NotNull(standardDictionaryGeneratorStubTypeBuilder);

            // static internal CreateStandardDictionaryGenerator(string key0, object value0, ... string keyN-1, object valyeN-1)
            Type[] argumentTypes = new Type[capacity * 2];
            for (ushort i = 0; i < capacity; i++)
            {
                argumentTypes[i * 2] = GenerationServices.StringType;
                argumentTypes[i * 2 + 1] = GenerationServices.ObjectType;
            }

            MethodBuilder standardDictionaryGeneratorBuilder = standardDictionaryGeneratorStubTypeBuilder.DefineMethod(
                capacity.ToString(CultureInfo.InvariantCulture),
                MethodAttributes.Static | MethodAttributes.Assembly,
                GenerationServices.standardDictionaryType,
                argumentTypes);

            ILGenerator ilGenerator = standardDictionaryGeneratorBuilder.GetILGenerator();

            //
            // Dictionary<string, object> metadata = new Dictionary<string, TValue>(capacity)
            //

            // create and load the dictionary
            GenerationServices.LoadInt(ilGenerator, capacity);
            ilGenerator.Emit(OpCodes.Newobj, GenerationServices.standardDictionaryConstructor);

            //
            //  dictionary.Add(key0, value0)
            //  dictionary.Add(key1, value1)
            // ... 
            //

            for (ushort i = 0; i < capacity; i++)
            {
                // the dictionary is on top of the stack - load it again
                ilGenerator.Emit(OpCodes.Dup);

                // load the key
                ilGenerator.Emit(OpCodes.Ldarg, i * 2);

                // load the value
                ilGenerator.Emit(OpCodes.Ldarg, i * 2 + 1);

                // Caal the "Add"
                ilGenerator.EmitCall(OpCodes.Call, GenerationServices.standardDictionaryAddMethod, null);
            }

            ilGenerator.Emit(OpCodes.Ret);

            return standardDictionaryGeneratorBuilder;
        }
    }
}
