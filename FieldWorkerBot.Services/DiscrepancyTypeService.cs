using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FieldWorkerBot.DataAccess;
using FieldWorkerBot.Domain;

namespace FieldWorkerBot.Services
{
    public class DiscrepancyTypeService
    {
        private DiscrepancyTypeRepository _discrepancyTypeRepository;

        public DiscrepancyTypeService(DiscrepancyTypeRepository discrepancyTypeRepository)
        {
            _discrepancyTypeRepository = discrepancyTypeRepository;
        }

        public async Task<IEnumerable<DiscrepancyType>> Get(string assetId, string inspectionId)
        {
            return await _discrepancyTypeRepository.Get(assetId, inspectionId);
        }
    }
}
