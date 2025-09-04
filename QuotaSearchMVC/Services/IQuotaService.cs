using QuotaSearchMVC.Models;

namespace QuotaSearchMVC.Services
{
    public interface IQuotaService
    {

        // Tuple yerine model sınıfı (QuotaConsumeResult) kullanıldı. 

        Task<QuotaConsumeResult> TryConsumeAsync(string userId, string query);

        Task<UsageInfoExtended> GetUsageAsync(string userId);

    }
}
