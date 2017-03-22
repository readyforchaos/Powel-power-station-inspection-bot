using System.Threading.Tasks;

namespace FieldWorkerBot.DataAccess.Infrastructure.SettingsManager
{
    public interface ISettingsManager
    {
        Task<string> GetAsync(string tenantName, string settingName, string region = null);
    }
}