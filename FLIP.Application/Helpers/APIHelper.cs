using System.Text.Json;
using FLIP.Application.Config;
using FLIP.Application.Models;

namespace FLIP.Application.Helpers;

public static class APIHelper
{
    public static List<ApiRequest> APIRequests()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "apiConfig.json");

        string json = File.ReadAllText(filePath);


        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var requests = JsonSerializer.Deserialize<List<ApiRequest>>(json, options);

        return requests ?? [];
    }
}
