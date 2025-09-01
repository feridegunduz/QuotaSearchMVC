using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuotaSearchMVC.Services;
using System.Security.Claims;

namespace QuotaSearchMVC.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly IQuotaService _quotaService;

        public SearchController(IQuotaService quotaService)
        {
            _quotaService = quotaService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Query(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Json(new { message = "Boş arama girdisi" });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var (success, usage) = await _quotaService.TryConsumeAsync(userId, query);

                if (!success)
                {
                    return StatusCode(429, new
                    {
                        success = false,
                        message = "Günlük veya aylık limitiniz dolmuştur!",
                        usage = new
                        {
                            dayRemaining = usage.DayRemaining,
                            monthRemaining = usage.MonthRemaining
                        }
                    });
                }


                var dummyResults = new List<string>
        {
            $"Sonuç 1: {query} hakkında bilgi.",
            $"Sonuç 2: {query} resimleri.",
            $"Sonuç 3: {query} makaleleri.",
            $"Sonuç 4: {query} videoları.",
            $"Sonuç 5: {query} forum tartışmaları."
        };

                return Json(new
                {
                    success = true,  // <-- Bunu ekledik
                    results = dummyResults,
                    usage = new
                    {
                        dayRemaining = usage.DayRemaining,
                        monthRemaining = usage.MonthRemaining
                    }
                });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    message = "Sunucu hatası",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }


        }

    }
}
