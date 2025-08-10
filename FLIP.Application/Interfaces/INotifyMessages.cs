using FLIP.Application.Models;

namespace FLIP.Application.Interfaces;

public interface INotifyMessages
{
    Task<Response> NotifyFLIPQeueuAsync(string freelancerID, bool isUpdate);
}
