using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FieldWorkerBot.DataAccess;
using FieldWorkerBot.Domain;

namespace FieldWorkerBot.Services
{
    public class InspectionService
    {
        private InspectionRepository _inspectionRepository;

        public InspectionService(InspectionRepository inspectionRepository)
        {
            _inspectionRepository = inspectionRepository;
        }

        public async Task<Inspection> GetNotPerformedByAssetId(string assetId)
        {
            return (await _inspectionRepository.GetByAssetId(assetId)).FirstOrDefault();
        }

        public async Task Commit(Inspection inspection)
        {
            await _inspectionRepository.Commit(inspection);
        }
    }
}
