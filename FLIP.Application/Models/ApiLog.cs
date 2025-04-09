namespace FLIP.Application.Models;

public class ApiLog
{
    public string RequestUri { get; set; } = default!;
    public int? StatusCode { get; set; }
    public string? Message { get; set; }
    public DateTimeOffset LoggedAt { get; set; }
    public double ResponseTimeMs { get; set; }
}
