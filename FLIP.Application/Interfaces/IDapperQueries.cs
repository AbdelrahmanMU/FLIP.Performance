using FLIP.Application.Models;

namespace FLIP.Application.Interfaces;

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
