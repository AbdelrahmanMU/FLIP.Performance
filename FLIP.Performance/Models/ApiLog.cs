namespace FLIP.Performance.Models;

public class ApiLog
{
    public string RequestUri { get; set; } = default!;
    public int? StatusCode { get; set; }
    public string? Message { get; set; }
    public DateTime LoggedAt { get; set; }
    public double ResponseTimeMs { get; set; }
}
