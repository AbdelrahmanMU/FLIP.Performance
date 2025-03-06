namespace FLIP.Performance.Models;

public class ErrorLogs
{
    public string? Component { get; set; }

    public string? Operation { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public string? RequestUrl { get; set; }

    public string? RequestPayload { get; set; }

    public string? ErrorMessage { get; set; }

    public string? StackTrace { get; set; }

    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}
