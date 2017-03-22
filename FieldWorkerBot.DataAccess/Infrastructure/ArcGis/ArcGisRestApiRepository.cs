using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FieldWorkerBot.DataAccess.Infrastructure.SettingsManager;
using Powel.ArcGIS.RestApi;

namespace FieldWorkerBot.DataAccess.Infrastructure.ArcGis
{
    public class ArcGisRestApiRepository<T> where T : class
    {
        private readonly string _proxyUrl;
        private readonly IRestApiFeatureSerializer<T> _featureSerializer;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISettingsManager _settingsManager;

        public ArcGisRestApiRepository(string proxyUrl, IRestApiFeatureSerializer<T> featureSerializer, IAuthorizationService authorizationService, ISettingsManager settingsManager)
        {
            if (featureSerializer == null) throw new ArgumentNullException(nameof(featureSerializer));
            if (authorizationService == null) throw new ArgumentNullException(nameof(authorizationService));
            if (settingsManager == null) throw new ArgumentNullException(nameof(settingsManager));
            _featureSerializer = featureSerializer;
            _authorizationService = authorizationService;
            _settingsManager = settingsManager;
            _proxyUrl = proxyUrl;
        }

        public async Task<IEnumerable<T>> WhereAsync(string tenantName, string predicate = "1=1")
        {
            var items = new List<T>();
            var offset = 0;
            const int pageSize = 2000;
            var moreData = true;

            while (moreData)
            {
                var nameValueCollection = new NameValueCollection
                {
                    {"where", predicate},
                    {"outfields", "*"},
                    {"returnGeometry", "true"},
                    {"resultOffset", offset.ToString()}
                };

                var res =
                    await GetRequest(await GetFeatureLayerUrl(tenantName), "query", nameValueCollection, tenantName, await GetToken(), await GetReferer(tenantName));
                items.AddRange(res.Features.Select(_featureSerializer.ToObject));

                if (res.Features.Length > 0)
                    offset += pageSize;
                else
                    moreData = false;
            }

            return items;
        }

        public async Task<long> AddAsync(string tenantName, T item)
        {
            return (await AddAsync(tenantName, new[] {item})).First();
        }

        public async Task<IEnumerable<long>> AddAsync(string tenantName, IEnumerable<T> items)
        {
            var fields = (await GetFieldInfo(tenantName)).ToArray();
            var nameValueCollection = new NameValueCollection
            {
                {"rollbackOnFailure", "true"},
                {"features", items.Select(item => _featureSerializer.ToFeature(item, fields)).ToJson()}
            };

            var res = await PostRequest(await GetFeatureLayerUrl(tenantName), "addFeatures", nameValueCollection, tenantName, await GetToken(), await GetReferer(tenantName));
            return res.AddResults.Select(r => r.ObjectId);
        }

        public async Task UpdateAsync(string tenantName, T item)
        {
            await UpdateAsync(tenantName, new[] {item});
        }

        public async Task UpdateAsync(string tenantName, IEnumerable<T> items)
        {
            var fields = (await GetFieldInfo(tenantName)).ToArray();
            var nameValueCollection = new NameValueCollection
            {
                {"rollbackOnFailure", "true"},
                {"features", items.Select(item => _featureSerializer.ToFeature(item, fields)).ToJson()}
            };

            await PostRequest(await GetFeatureLayerUrl(tenantName), "updateFeatures", nameValueCollection, tenantName, await GetToken(), await GetReferer(tenantName));
        }

        private async Task<IEnumerable<Field>> GetFieldInfo(string tenantName)
        {
            var res = await GetRequest(await GetFeatureLayerUrl(tenantName), "", new NameValueCollection(), tenantName, await GetToken(), await GetReferer(tenantName));
            return res.Fields;
        }

        private async Task<FeatureQueryResult> GetRequest(string url, string resource, NameValueCollection parameters, string tenant, string bearer, string referer)
        {
            var resourceUrl = string.IsNullOrEmpty(resource) ? url : url + "/" + resource;
            var result = await GetRequest(resourceUrl, parameters, tenant, bearer, referer);
            if (result.Error != null) throw new WebException(result.Error.Message);
            return result;
        }

        private async Task<UpdateResult> PostRequest(string url, string resource, NameValueCollection parameters, string tenant, string bearer, string referer)
        {
            var resourceUrl = string.IsNullOrEmpty(resource) ? url : url + "/" + resource;
            var result = await PostRequest(resourceUrl, parameters, tenant, bearer, referer);
            if (result.Error != null) throw new WebException(result.Error.Message);
            return result;
        }

        private async Task<FeatureQueryResult> GetRequest(string url, NameValueCollection parameters, string tenant, string bearer, string referer)
        {
            var client = new WebClient { Encoding = Encoding.UTF8 };
           
            parameters.Add("f", "json");
            parameters.Add("tenant", tenant);

            url = url + "?" + parameters.ToQueryString();
            if (!string.IsNullOrEmpty(_proxyUrl)) url = _proxyUrl + "?" + url;

            client.Headers.Add(HttpRequestHeader.Referer, referer);
            client.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {bearer}");

            string response = await client.DownloadStringTaskAsync(url);
            var result = response.FromJson<FeatureQueryResult>();
            return result;
        }

        private async Task<UpdateResult> PostRequest(string url, NameValueCollection parameters, string tenant, string bearer, string referer)
        {
            var client = new WebClient { Encoding = Encoding.UTF8 };

            if (!string.IsNullOrEmpty(_proxyUrl)) url = _proxyUrl + "?" + url;
            parameters.Add("f", "json");
            parameters.Add("tenant", tenant);

            client.Headers.Add(HttpRequestHeader.Referer, referer);
            client.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {bearer}");

            var responseBytes = await client.UploadValuesTaskAsync(url, parameters);
            var response = Encoding.UTF8.GetString(responseBytes);
            var result = response.FromJson<UpdateResult>();
            return result;
        }

        private async Task<string> GetToken()
        {
            return await _authorizationService.GetToken();
        }

        private async Task<string> GetFeatureLayerUrl(string tenantName)
        {
            return await _settingsManager.GetAsync(tenantName, "objectFeatureLayerUrl");
        }

        private async Task<string> GetReferer(string tenantName)
        {
            return await _settingsManager.GetAsync(tenantName, "objectFeatureLayerReferer");
        }
    }

    public static class ArcGisRestApiRepositoryHelpers
    {
        public static string ToQueryString(this NameValueCollection source, bool removeEmptyEntries = true)
        {
            var array = (from key in source.AllKeys
                from value in source.GetValues(key)
                select $"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}")
                .ToArray();
            return string.Join("&", array);
        }
    }
}
