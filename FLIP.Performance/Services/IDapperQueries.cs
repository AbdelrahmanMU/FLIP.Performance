using FLIP.Performance.Models;

namespace FLIP.Performance.Services;

public interface IDapperQueries
{
    Task<int> InsertFreeelancers(List<FreelancerData> freelancersData); 
    Task<int> InsertLogs(List<ApiLog> apiLogs); 
    Task<int> InsertErrorLogs(List<ErrorLogs> apiLogs); 
}
