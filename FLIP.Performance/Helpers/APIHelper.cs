using FLIP.Performance.Config;
using System.Text.Json;

namespace FLIP.Performance.Helpers;

public static class APIHelper
{
    public static List<ApiRequest> APIRequests()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "apiConfig.json");

        string json = File.ReadAllText(filePath);


        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // Enable case-insensitive deserialization
        };

        var requests = JsonSerializer.Deserialize<List<ApiRequest>>(json, options);

        return requests ?? [];
    }

}
