namespace QuotaSearchMVC.Models
{
    public class QuotaConsumeResult
    {
        public bool Success { get; set; }
        public UsageInfo Usage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
