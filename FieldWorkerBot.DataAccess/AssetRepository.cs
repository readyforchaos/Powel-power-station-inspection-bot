using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FieldWorkerBot.DataAccess.Infrastructure.ArcGis;
using FieldWorkerBot.DataAccess.Infrastructure.AzureStorage;
using FieldWorkerBot.DataAccess.Infrastructure.SettingsManager;
using FieldWorkerBot.DataAccess.InternalDomain;
using FieldWorkerBot.Domain;
using Powel.WebApi.Client;

namespace FieldWorkerBot.DataAccess
{
    public class AssetRepository
    {
        private ArcGisRestApiRepository<ObjectItem> _objectItemRepository;

        public AssetRepository()
        {
            var authManager = new ClientCredentialsAuthorizationManager(
                "",
                "", 
                "", 
                "");
            var settingsStorage =
                new AzureTableStorage(
                    "",
                    "", 
                    "");
            _objectItemRepository =
                new ArcGisRestApiRepository<ObjectItem>(
                    "",
                    new ObjectRestApiFeatureSerializer(), new BackchannelAuthorizationService(authManager),
                    new SettingsManager(settingsStorage, new DefaultMemoryCache()));
        }

        public async Task<Asset> GetByName(string name)
        {
            var objectItem = await GetByNameInternal(name);
            return objectItem == null ? null : MapObjectItem(objectItem);
        }

        internal async Task SetInspected(string assetId, int statusCode)
        {
            var objectItem = await GetByIdInternal(assetId);
            objectItem.CurrentStatusCode = statusCode;
            objectItem.ObservationCount = statusCode;
            var inspectionInfo = objectItem.InspectionInfos.FirstOrDefault(i => i.InspectionPlanned);
            if (inspectionInfo != null)
            {
                inspectionInfo.InspectionPlanned = false;
                inspectionInfo.StatusCode = statusCode;
                inspectionInfo.LastInspectionDate = DateTime.Now;
                inspectionInfo.NextInspectionDate = null;
            }

            await SaveInternal(objectItem);
        }

        internal async Task<ObjectItem> GetByNameInternal(string name)
        {
            return (await _objectItemRepository.WhereAsync("demo", $"Name='{name}'")).FirstOrDefault(); ;
        }

        internal async Task<ObjectItem> GetByIdInternal(string id)
        {
            return (await _objectItemRepository.WhereAsync("demo", $"ExternalId='{id}'")).FirstOrDefault(); ;
        }

        internal async Task SaveInternal(ObjectItem objectItem)
        {
            await _objectItemRepository.UpdateAsync("demo", objectItem);
        }

        private Asset MapObjectItem(ObjectItem objectItem)
        {
            if (objectItem == null) throw new ArgumentNullException(nameof(objectItem));

            return new Asset
            {
                Id = objectItem.Id,
                ObjectId = objectItem.ExternalId,
                Name = objectItem.Name,
                CurrentStatusCode = objectItem.CurrentStatusCode
            };
        }
    }
}
