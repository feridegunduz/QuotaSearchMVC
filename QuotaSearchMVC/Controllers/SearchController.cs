using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuotaSearchMVC.Services;
using System.Security.Claims;

namespace QuotaSearchMVC.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly IQuotaService _quotaService;

        public SearchController(IQuotaService quotaService)
        {
            _quotaService = quotaService;
        }

        public class SearchRequest
        {
            public string Term { get; set; } = string.Empty;
        }

        [HttpPost]
public async Task<IActionResult> Post([FromBody] SearchRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Term))
        return BadRequest(new { message = "Boş arama girdisi" });

    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var result = await _quotaService.TryConsumeAsync(userId, request.Term);

    // Header ekleme
    Response.Headers["X-RateLimit-Limit-Day"] = "5";
    Response.Headers["X-RateLimit-Remaining-Day"] = result.Usage.DayRemaining.ToString();
    Response.Headers["X-RateLimit-Limit-Month"] = "20";
    Response.Headers["X-RateLimit-Remaining-Month"] = result.Usage.MonthRemaining.ToString();

    if (!result.Success)
    {
        string code, message;

        if (result.Usage.DayRemaining == 0 && result.Usage.MonthRemaining == 0)
        {
            code = "BOTH_LIMITS_EXCEEDED";
            message = "Günlük ve aylık limitleriniz dolmuştur.";
        }
        else if (result.Usage.DayRemaining == 0)
        {
            code = "DAILY_LIMIT_EXCEEDED";
            message = "Günlük limitiniz dolmuştur.";
        }
        else if (result.Usage.MonthRemaining == 0)
        {
            code = "MONTHLY_LIMIT_EXCEEDED";
            message = "Aylık limitiniz dolmuştur.";
        }
        else
        {
            code = "LIMIT_EXCEEDED";
            message = "Limitiniz dolmuştur.";
        }

        return StatusCode(429, new { code, message });
    }

    // Dummy sonuçlar
    var items = new List<string>
    {
        $"Sonuç 1: {request.Term} hakkında bilgi.",
        $"Sonuç 2: {request.Term} resimleri.",
        $"Sonuç 3: {request.Term} makaleleri."
    };

    return Ok(new { items, usage = new { dayRemaining = result.Usage.DayRemaining, monthRemaining = result.Usage.MonthRemaining } });
}


        [HttpGet("/api/usage")]
        public async Task<IActionResult> GetUsage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var usage = await _quotaService.GetUsageAsync(userId);

            return Ok(new
            {
                dayUsed = 5 - usage.DayRemaining,
                dayRemaining = usage.DayRemaining,
                monthUsed = 20 - usage.MonthRemaining,
                monthRemaining = usage.MonthRemaining,
                dayResetAt = usage.DayResetAt,
                monthResetAt = usage.MonthResetAt
            });
        }
    }
}



