using FLIP.Application.Models;

namespace FLIP.Application.Interfaces;

public interface IDapperQueries
{
    Task<int> InsertFreelancers(FreelancerData freelancersData);
    Task<int> InsertFreelancerRide(FreelancerData freelancersData);
    Task<int> InsertLogs(ApiLog apiLogs);
    Task<int> InsertErrorLogs(ErrorLogs apiLogs);
    Task<List<string>> GetFreelancersIds();

    Task<int> UpdateFreelancers(FreelancerData freelancersData);
    Task<int> UpdateFreelancersRide(FreelancerData freelancersData);
}
