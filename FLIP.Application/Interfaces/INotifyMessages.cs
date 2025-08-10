using FLIP.Application.Models;

namespace FLIP.Application.Interfaces;

public interface INotifyMessages
{
    Task<Response> NotifyFLIPRealTimeQeueuAsync(string freelancerID);
    Task<Response> NotifyDailyJobQeueuAsync(string freelancerID);
}
