namespace FLIP.Application.Config;

public class ApiRequest
{
    public string PlatformName { get; set; } = default!;
    public string Url { get; set; } = default!;
    public bool IsBearerAuth { get; set; }
    public string? BearerToken { get; set; }
    public List<string> Params { get; set; } = [];
}
