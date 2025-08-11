using FLIP.Application.Models;

namespace FLIP.Application.Interfaces;

public interface INotifyMessages
{
    Task<ResponseVM<List<PlatformMeta>>> NotifyFLIPRealTimeQeueuAsync(string freelancerID);
    Task<Response> NotifyDailyJobQeueuAsync(List<FreelancerDailyJobDto> freelancersDto);
}
