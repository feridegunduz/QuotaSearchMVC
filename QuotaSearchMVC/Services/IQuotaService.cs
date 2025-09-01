using QuotaSearchMVC.Models;

namespace QuotaSearchMVC.Services
{
    public interface IQuotaService
    {
        Task<(bool success, UsageInfo usage)> TryConsumeAsync(string userId, string query);
    }
}
