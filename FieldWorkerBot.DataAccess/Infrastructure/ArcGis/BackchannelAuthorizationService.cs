using System;
using System.Threading.Tasks;
using Powel.WebApi.Client.Interfaces;

namespace FieldWorkerBot.DataAccess.Infrastructure.ArcGis
{
    public class BackchannelAuthorizationService : IAuthorizationService
    {
        private readonly IAuthorizationManager _authorizationManager;

        public BackchannelAuthorizationService(IAuthorizationManager authorizationManager)
        {
            if (authorizationManager == null) throw new ArgumentNullException(nameof(authorizationManager));
            _authorizationManager = authorizationManager;
        }

        public async Task<string> GetToken()
        {
            return await _authorizationManager.GetAccessToken();
        }
    }
}
