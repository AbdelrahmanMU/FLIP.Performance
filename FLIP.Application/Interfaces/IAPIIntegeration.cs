using FLIP.Application.Commands.ProcessId;
using FLIP.Application.Models;

namespace FLIP.Application.Interfaces;

public interface IAPIIntegeration
{
    Task<Response> ProcessId(FreelancerDto request);
}
