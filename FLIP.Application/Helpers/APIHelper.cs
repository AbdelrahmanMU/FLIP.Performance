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
            PropertyNameCaseInsensitive = true // Enable case-insensitive deserialization
        };

        var requests = JsonSerializer.Deserialize<List<ApiRequest>>(json, options);

        return requests ?? [];
    }

    public static List<FreelancerData> DumyData()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "DummyData.json");

        string json = File.ReadAllText(filePath);

        using JsonDocument document = JsonDocument.Parse(json);
        var freelancers = new List<FreelancerData>();

        foreach (var element in document.RootElement.EnumerateArray())
        {
            var freelancer = new FreelancerData
            {
                TransactionID = element.GetProperty("TransactionID").GetGuid(),
                PlatformName = element.GetProperty("PlatformName").GetString()!,
                NationalId = element.GetProperty("NationalId").GetInt64().ToString(), // Keeping as string
                JsonContent = element.GetProperty("JsonContent").GetRawText() // Serialize JsonContent as a string
            };

            freelancers.Add(freelancer);
        }

        return freelancers ?? [];
    }

}
