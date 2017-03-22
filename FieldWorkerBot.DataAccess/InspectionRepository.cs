using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FieldWorkerBot.DataAccess.InternalDomain;
using FieldWorkerBot.Domain;
using Powel.WebApi.Client;

namespace FieldWorkerBot.DataAccess
{
    public class InspectionRepository
    {
        private AssetRepository _assetRepository;
        private WebApiClient _webApiClient;

        public InspectionRepository(AssetRepository assetRepository)
        {
            _assetRepository = assetRepository;

            var authManager = new ClientCredentialsAuthorizationManager(
                "",
                "",
                "",
                "");

            _webApiClient = new WebApiClient(new BearerTokenWebApiCaller(authManager));
        }

        public async Task<IEnumerable<Inspection>> GetByAssetId(string assetId)
        {
            return (await GetByAssetIdInternal(assetId)).Select(MapInspectionJob);
        }

        public async Task Commit(Inspection inspection)
        {
            var inspectionJobs = (await GetByAssetIdInternal(inspection.AssetId)).ToList();
            var inspectionJob =
                inspectionJobs.FirstOrDefault(i => i.Id == inspection.Id);
            inspectionJob.InspectionDate = inspection.InspectionDate;
            inspectionJob.StatusCode = inspection.StatusCode;
            await SaveInternal(inspectionJob);
            
            await _assetRepository.SetInspected(inspection.AssetId, inspection.StatusCode);
        }

        internal async Task<IEnumerable<InspectionJob>> GetByAssetIdInternal(string assetId)
        {
            var response = await _webApiClient.Get<IEnumerable<InspectionJob>>("",
                $"demo/objects/{assetId}/inspection-jobs");
            return response.Result;
        }

        internal async Task SaveInternal(InspectionJob inspectionJob)
        {
            var response =
                await _webApiClient.Post(
                    "",
                    "demo/inspection-jobs", inspectionJob);
        }

        private Inspection MapInspectionJob(InspectionJob inspectionJob)
        {
            return new Inspection
            {
                Id = inspectionJob.Id,
                AssetId = inspectionJob.ObjectId,
                InspectionDate = inspectionJob.InspectionDate,
                StatusCode = inspectionJob.StatusCode
            };
        }
    }
}
