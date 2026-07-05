using System.Collections.Concurrent;
using System.Collections.Generic;
using Oxide.Ext.UiFramework.Plugins;
using Oxide.Ext.UiFramework.Pooling;

namespace Oxide.Ext.UiFramework.Extensions;

public static class ConcurrentDictionaryExt
{
    extension<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dictionary)
    {
        public UiPooledArray<KeyValuePair<TKey, TValue>> ToArrayPooled(IUiFrameworkPlugin plugin) => dictionary.ToArrayPooled(plugin, dictionary.Count);
        
        public UiPooledArray<KeyValuePair<TKey, TValue>> ToArrayPooled(IUiFrameworkPlugin plugin, int count)
        {
            //The dictionary may be mutated from another thread (e.g. the main thread enqueuing
            //animations while the animation thread reads them). ICollection.CopyTo re-validates the
            //destination against the dictionary's CURRENT count under lock and throws if it grew past
            //the sampled count, so instead enumerate the lock-free moving snapshot and copy up to the
            //pooled array's capacity. Any entries added past capacity are processed on the next pass.
            UiPooledArray<KeyValuePair<TKey, TValue>> array = plugin.PluginPool.GetArray<KeyValuePair<TKey, TValue>>(count * 2);
            KeyValuePair<TKey, TValue>[] buffer = array.Array;
            int index = 0;
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                if (index >= buffer.Length)
                    break;

                buffer[index++] = pair;
            }

            array.WithLength(index);
            return array;
        }

        public PooledDictionaryEnumerator<TKey, TValue> GetEnumeratorPooled(IUiFrameworkPlugin plugin)
        {
            return PooledDictionaryEnumerator<TKey, TValue>.Create(plugin, dictionary.ToArrayPooled(plugin));
        }
    }
}