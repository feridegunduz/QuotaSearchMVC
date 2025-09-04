using Microsoft.EntityFrameworkCore;
using QuotaSearchMVC.Data;
using QuotaSearchMVC.Models;

namespace QuotaSearchMVC.Services
{
    public class QuotaService : IQuotaService
    {
        private readonly AppDbContext _context;
        private readonly TimeSpan _tzOffset = TimeSpan.FromHours(3); // İstanbul

        private const int DailyLimit = 5;
        private const int MonthlyLimit = 20;

        public QuotaService(AppDbContext context)
        {
            _context = context;
        }

        // Arama sorgusunu çalıştır ve limitleri uygula
        public async Task<QuotaConsumeResult> TryConsumeAsync(string userId, string query)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var utcNow = DateTime.UtcNow;
            var localNow = utcNow + _tzOffset;

            var dayStartLocal = new DateTime(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0);
            var dayEndLocal = dayStartLocal.AddDays(1);
            var monthStartLocal = new DateTime(localNow.Year, localNow.Month, 1, 0, 0, 0);
            var monthEndLocal = monthStartLocal.AddMonths(1);

            var dayStartUtc = dayStartLocal - _tzOffset;
            var dayEndUtc = dayEndLocal - _tzOffset;
            var monthStartUtc = monthStartLocal - _tzOffset;
            var monthEndUtc = monthEndLocal - _tzOffset;

            using var tx = await _context.Database.BeginTransactionAsync();

            var dailyCount = await _context.QueryLogs
                .Where(q => q.UserId == userId && q.CreatedAtUtc >= dayStartUtc && q.CreatedAtUtc < dayEndUtc)
                .CountAsync();

            var monthlyCount = await _context.QueryLogs
                .Where(q => q.UserId == userId && q.CreatedAtUtc >= monthStartUtc && q.CreatedAtUtc < monthEndUtc)
                .CountAsync();

            if (dailyCount >= DailyLimit)
            {
                await tx.RollbackAsync();
                return new QuotaConsumeResult
                {
                    Success = false,
                    Usage = new UsageInfo
                    {
                        DayRemaining = 0,
                        MonthRemaining = Math.Max(0, MonthlyLimit - monthlyCount)
                    },
                    ErrorMessage = "Günlük limit aşıldı."
                };
            }

            if (monthlyCount >= MonthlyLimit)
            {
                await tx.RollbackAsync();
                return new QuotaConsumeResult
                {
                    Success = false,
                    Usage = new UsageInfo
                    {
                        DayRemaining = Math.Max(0, DailyLimit - dailyCount),
                        MonthRemaining = 0
                    },
                    ErrorMessage = "Aylık limit aşıldı."
                };
            }

            var log = new QueryLog
            {
                UserId = userId,
                QueryText = query,
                CreatedAtUtc = utcNow
            };

            _context.QueryLogs.Add(log);
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            return new QuotaConsumeResult
            {
                Success = true,
                Usage = new UsageInfo
                {
                    DayRemaining = Math.Max(0, DailyLimit - (dailyCount + 1)),
                    MonthRemaining = Math.Max(0, MonthlyLimit - (monthlyCount + 1))
                }
            };
        }

        // Kullanım bilgilerini döndür
        public async Task<UsageInfoExtended> GetUsageAsync(string userId)
        {
            var utcNow = DateTime.UtcNow;
            var localNow = utcNow + _tzOffset;

            var dayStartLocal = new DateTime(localNow.Year, localNow.Month, localNow.Day, 0, 0, 0);
            var monthStartLocal = new DateTime(localNow.Year, localNow.Month, 1, 0, 0, 0);

            var dayStartUtc = dayStartLocal - _tzOffset;
            var monthStartUtc = monthStartLocal - _tzOffset;

            var dailyCount = await _context.QueryLogs
                .Where(q => q.UserId == userId && q.CreatedAtUtc >= dayStartUtc)
                .CountAsync();

            var monthlyCount = await _context.QueryLogs
                .Where(q => q.UserId == userId && q.CreatedAtUtc >= monthStartUtc)
                .CountAsync();

            return new UsageInfoExtended
            {
                DayRemaining = Math.Max(0, DailyLimit - dailyCount),
                MonthRemaining = Math.Max(0, MonthlyLimit - monthlyCount),
                DayResetAt = dayStartLocal.AddDays(1),
                MonthResetAt = monthStartLocal.AddMonths(1)
            };
        }
    }

    // Extended UsageInfo, reset zamanları ile birlikte
    public class UsageInfoExtended : UsageInfo
    {
        public DateTime DayResetAt { get; set; }
        public DateTime MonthResetAt { get; set; }
    }
}
