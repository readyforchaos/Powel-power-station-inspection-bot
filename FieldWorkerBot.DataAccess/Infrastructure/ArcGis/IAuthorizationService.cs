using System.Threading.Tasks;

namespace FieldWorkerBot.DataAccess.Infrastructure.ArcGis
{
    public interface IAuthorizationService
    {
        Task<string> GetToken();
    }
}
