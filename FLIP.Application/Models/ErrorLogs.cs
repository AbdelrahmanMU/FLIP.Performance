namespace FLIP.Application.Models;

public class ErrorLogs
{
    public Guid Id { get; set; }
    public string? Component { get; set; }

    public string? Operation { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public string? RequestUrl { get; set; }

    public string? RequestPayload { get; set; }

    public string? ErrorMessage { get; set; }

    public string? StackTrace { get; set; }

    public DateTimeOffset LoggedAt { get; set; } = DateTime.Now;
}
