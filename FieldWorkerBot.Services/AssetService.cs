using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FieldWorkerBot.DataAccess;
using FieldWorkerBot.Domain;

namespace FieldWorkerBot.Services
{
    public class AssetService
    {
        private AssetRepository _assetRepository;

        public AssetService(AssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
        }

        public async Task<Asset> GetByName(string name)
        {
            return await _assetRepository.GetByName(name);
        }
    }
}
