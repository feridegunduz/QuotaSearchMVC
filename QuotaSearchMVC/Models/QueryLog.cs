namespace QuotaSearchMVC.Models
{
    public class QueryLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string QueryText { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
