using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.ComponentModel.Composition.DynamicInstantiation
{
    class ConcurrentCache<TKey, TValue>
    {
        IDictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();
        object _synchRoot = new object();

        public TValue GetOrCreate(TKey key, Func<TValue> valueCreator)
        {
            lock (_synchRoot)
            {
                TValue value;
                if (!_cache.TryGetValue(key, out value))
                    _cache[key] = value = valueCreator();

                return value;
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                lock (_synchRoot)
                    return _cache.Values.ToArray();
            }
        }
    }
}
