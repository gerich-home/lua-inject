// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Internal.Collections;

namespace System.ComponentModel.Composition.Caching.AttributedModel
{
    internal static partial class AttributedCacheServices
    {
        public static void WriteValue<T>(this IDictionary<string, object> cache, string key, T value, T defaultValue)
        {
            if (object.Equals(value, defaultValue))
            {
                return;
            }

            cache.Add(key, value);
        }

        public static void WriteValue<T>(this IDictionary<string, object> cache, string key, T value)
        {
            cache.WriteValue<T>(key, value, default(T));
        }

        public static void WriteDictionary<T>(this IDictionary<string, object> cache, string key, IDictionary<string, T> dictionary)
        {
            if ((dictionary != null) && (dictionary.Count > 0))
            {
                cache.WriteValue(key, dictionary);
            }
        }

        public static void WriteEnumerable<T>(this IDictionary<string, object> cache, string key, IEnumerable<T> enumerable)
        {
            if ((enumerable != null) && enumerable.Any())
            {
                cache.WriteValue(key, enumerable);
            }
        }

        public static T ReadValue<T>(this IDictionary<string, object> cache, string key, T defaultValue)
        {
            object value = null;
            if (cache.TryGetValue(key, out value))
            {
                return (T)value;
            }
            else
            {
                return defaultValue;
            }
        }

        public static T ReadValue<T>(this IDictionary<string, object> cache, string key)
        {
            return cache.ReadValue<T>(key, default(T));
        }

        public static IDictionary<string, T> ReadDictionary<T>(this IDictionary<string, object> cache, string key)
        {
            IDictionary<string, T> result = cache.ReadValue<IDictionary<string, T>>(key, null);
            if (result != null)
            {
                return result;
            }

            if (typeof(object) == typeof(T))
            {
                return (IDictionary<string, T>)(object)MetadataServices.EmptyMetadata;
            }
            else
            {
                return new Dictionary<string, T>();
            }
        }

        public static IEnumerable<T> ReadEnumerable<T>(this IDictionary<string, object> cache, string key)
        {
            return cache.ReadValue<IEnumerable<T>>(key, Enumerable.Empty<T>());
        }
    }
}
