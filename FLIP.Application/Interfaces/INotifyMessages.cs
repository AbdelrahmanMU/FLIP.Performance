using FLIP.Application.Models;

namespace FLIP.Application.Interfaces;

public interface INotifyMessages
{
    Task NotifyFLIPRealTimeAsync(string number);
    Task NotifyDailyJobAsync(int number, Response apiResponse);
}
