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
    public class DiscrepancyTypeRepository
    {
        private AssetRepository _assetRepository;
        private InspectionRepository _inspectionRepository;
        private WebApiClient _webApiClient;

        public DiscrepancyTypeRepository(InspectionRepository inspectionRepository, AssetRepository assetRepository)
        {
            _inspectionRepository = inspectionRepository;
            _assetRepository = assetRepository;

            var authManager = new ClientCredentialsAuthorizationManager(
                "",
                "",
                "",
                "");

            _webApiClient = new WebApiClient(new BearerTokenWebApiCaller(authManager));
        }

        public async Task<IEnumerable<DiscrepancyType>> Get(string assetId, string inspectionId)
        {
            var asset = await _assetRepository.GetByIdInternal(assetId);
            var inspection = (await _inspectionRepository.GetByAssetIdInternal(assetId)).FirstOrDefault(i=>i.Id==inspectionId);
            var jobSpecification = await GetInternal(asset.ObjectType, inspection.InspectionType,
                inspection.JobSpecificationVersion);

            return
                jobSpecification.ControlPoints.Select(
                    p => new DiscrepancyType {Id = p.Id, Name = p.Name, Type = p.ControlPointStatusType});
        }

        internal async Task<JobSpecification> GetInternal(string assetType, string inspectionType,
            string jobSpecificationVersion)
        {
            var response =
                await _webApiClient.Get<IEnumerable<JobSpecification>>("",
                    $"demo/job-specifications?inspectionObjectType={assetType}&inspectionType={inspectionType}&version={jobSpecificationVersion}");
            return response.Result.FirstOrDefault();
        }
    }
}
