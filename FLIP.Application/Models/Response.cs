using System.Text.Json.Serialization;

namespace FLIP.Application.Models;

public class Response
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? Errors { get; set; }

    [JsonIgnore] public FreelancerData? FreelancerData { get; set; } = new();
    [JsonIgnore] public ApiLog ApiLogData { get; set; } = new();
    [JsonIgnore] public ErrorLogs? ErrorLogsData { get; set; } = new();

    public List<PlatformMeta> PlatformsMeta { get; set; } = [];
}

public class PlatformMeta
{
    public string PlatformName { get; set; } = default!;
    public bool IsSuccessfullyPublished { get; set; }
}