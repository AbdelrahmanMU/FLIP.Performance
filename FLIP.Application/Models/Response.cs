namespace FLIP.Application.Models;

public class Response
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public List<string?> Errors { get; set; } = [];
}
