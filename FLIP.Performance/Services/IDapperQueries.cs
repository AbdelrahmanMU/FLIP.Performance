using FLIP.Performance.Models;

namespace FLIP.Performance.Services;

public interface IDapperQueries
{
    Task<int> InsertFreelancers(List<FreelancerData> freelancersData); 
    Task<int> InsertFreelancersRide(List<FreelancerData> freelancersData); 
    Task<int> InsertLogs(List<ApiLog> apiLogs); 
    Task<int> InsertErrorLogs(List<ErrorLogs> apiLogs); 
    Task<List<string>> GetFreelancersIds();

    Task<int> UpdateFreelancers(List<FreelancerData> freelancersData);
    Task<int> UpdateFreelancersRide(List<FreelancerData> freelancersData);
}
