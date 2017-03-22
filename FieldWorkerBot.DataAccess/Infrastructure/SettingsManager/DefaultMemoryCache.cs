using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace FieldWorkerBot.DataAccess.Infrastructure.SettingsManager
{
    public class DefaultMemoryCache : ISettingsCache
    {
        private static readonly MemoryCache Cache = MemoryCache.Default;

        public T Get<T>(string key, string region = null) where T : class
        {
            var o = Cache.Get(CombineKeyWithRegion(key, region)) as T;
            return o;
        }

        public void Remove(string key, string region = null)
        {
            lock (Cache)
            {
                Cache.Remove(CombineKeyWithRegion(key, region));
            }
        }

        public bool Contains(string key, string region = null)
        {
            return Cache.Contains(CombineKeyWithRegion(key, region));
        }

        public void Add(string key, object o, DateTimeOffset expiration, string region = null, string dependsOnKey = null)
        {
            var cachePolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = expiration
            };

            if (!string.IsNullOrWhiteSpace(dependsOnKey))
            {
                cachePolicy.ChangeMonitors.Add(
                    Cache.CreateCacheEntryChangeMonitor(new[] {dependsOnKey})
                    );
            }
            lock (Cache)
            {
                Cache.Add(CombineKeyWithRegion(key, region), o, cachePolicy);
            }
        }

        public IEnumerable<string> AllKeys
        {
            get { return Cache.Select(x => x.Key); }
        }

        private string CombineKeyWithRegion(string key, string region = "no_region")
        {
            return region + ";" + key;
        }

        public void ClearAll()
        {
            foreach (var element in Cache)
            {
                Cache.Remove(element.Key);
            }
        }
    }
}