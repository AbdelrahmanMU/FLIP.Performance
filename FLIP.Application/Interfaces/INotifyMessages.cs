using FLIP.Application.Models;

namespace FLIP.Application.Interfaces;

public interface INotifyMessages
{
    Task NotifyBREAsync(int number, Response apiResponse);
}
