using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Globalization;

namespace Microsoft.Internal
{
    internal static class LazyServices
    {
        public static Lazy<T> AsLazy<T>(this T t)
            where T : class
        {
            return new Lazy<T>(() => t, false);
        }

        public static Lazy<T> MakeLazy<T>(Func<T> func)
            where T : class
        {
            return (func != null) ? new Lazy<T>(func) : new Lazy<T>(() => (T)null, false);
        }

        public static Lazy<T> MakeLazy<T>(Func<T> func, IDictionary<string, object> metadata)
            where T : class 
        {
            return (func != null) ? new LazyInitWithMetadata<T>(func, metadata) : new LazyInitWithMetadata<T>(() => (T)null, metadata);
        }

        public static IDictionary<string, object> GetMetadata<T>(this Lazy<T> lazy)
        {
            LazyInitWithMetadata<T> lazyWithMetadata = lazy as LazyInitWithMetadata<T>;
            if (lazyWithMetadata != null)
            {
                return lazyWithMetadata.Metadata;
            }
            else
            {
                return null;
            }
        }

        public static T GetValueAllowNullLazy<T>(this Lazy<T> lazy)
            where T : class
        {
            if (lazy != null)
            {
                return lazy.Value;
            }
            else
            {
                return null;
            }
        }


        private class LazyInitWithMetadata<T> : Lazy<T>
        {
            private IDictionary<string, object> _metadata;

            public LazyInitWithMetadata(Func<T> creator, IDictionary<string, object> metadata) : 
                base(creator)
            {
                this._metadata = metadata;
            }

            public IDictionary<string, object> Metadata
            {
                get
                {
                    return this._metadata;
                }
            }
        }
    }
}
