using FLIP.Performance.Config;

namespace FLIP.Performance.Helpers;

public static class Extensions
{
    public static ApiRequest PrepareParams(this ApiRequest api)
    {
        var formatedParams = string.Join("/", api.Params);

        api.Url = api.Url + formatedParams;

        return api;
    }
}
