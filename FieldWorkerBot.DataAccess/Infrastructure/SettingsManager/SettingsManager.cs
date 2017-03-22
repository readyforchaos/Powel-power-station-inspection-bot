using System;
using System.Threading.Tasks;
using FieldWorkerBot.DataAccess.Infrastructure.AzureStorage;
using Newtonsoft.Json.Linq;

namespace FieldWorkerBot.DataAccess.Infrastructure.SettingsManager
{
    public class SettingsManager : ISettingsManager
    {
        private ITableStorage _storage;
        private ISettingsCache _cache;
        private int _cacheAbsoluteExpirationInHours ;

        public SettingsManager(ITableStorage storage, ISettingsCache cache, int cacheAbsoluteExpirationInHours = 24)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");
            _storage = storage;

            if (cache == null)
                throw new ArgumentNullException("cache");
            _cache = cache;

            _cacheAbsoluteExpirationInHours = cacheAbsoluteExpirationInHours;
        }


        public async Task<string> GetAsync(string tenantName, string settingName, string region = null)
        {
            if (string.IsNullOrEmpty(settingName))
                throw new ArgumentNullException("key");

            if (_cache.Contains(settingName, region))
            {
                return _cache.Get<string>(settingName, region);
            }

            TenantInfoEntity entity;
            if (_cache.Contains(tenantName))
            {
                entity = _cache.Get<TenantInfoEntity>(tenantName);
            }
            else
            {
                entity = await _storage.GetAsync<TenantInfoEntity>(tenantName);
            }

            var jObject = JObject.Parse(entity.Data);

            var settingsObject = jObject["settings"] as JObject;

            JToken jToken;

            if (settingsObject.TryGetValue(settingName, StringComparison.OrdinalIgnoreCase, out jToken))
            {
                string value = jToken.Value<string>();
                _cache.Add(settingName, value, DateTime.Now.AddHours(_cacheAbsoluteExpirationInHours));

                return value;
            }

            return null;
        }
    }
}
