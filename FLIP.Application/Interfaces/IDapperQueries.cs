using FLIP.Application.Models;
using FLIP.Application.Queries.GetFreelancerData;

namespace FLIP.Application.Interfaces;

public interface IDapperQueries
{
    Task<int> InsertFreelancers(FreelancerData? freelancersData);
    Task<int> InsertFreelancerRide(FreelancerData? freelancersData);
    Task<int> InsertLogs(ApiLog apiLogs);
    Task<int> InsertErrorLogs(ErrorLogs? apiLogs);
    Task<List<FreelancerDailyJobDto>> GetFreelancersProjectsUpdateInfo();
    Task<List<FreelancerDailyJobDto>> GetFreelancersRidesUpdateInfo();

    Task<int> UpdateFreelancersProjects(FreelancerData? freelancersData);
    Task<int> UpdateFreelancersRide(FreelancerData? freelancersData);

    Task<List<GetFreelancerDto>> GetFreelancerData(string freelancerId);
}
