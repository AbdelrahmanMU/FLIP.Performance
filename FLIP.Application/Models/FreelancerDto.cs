using FLIP.Application.Config;

namespace FLIP.Application.Models;

public class FreelancerDto
{
    public string Id { get; set; } = default!;
    public ApiRequest Api { get; set; } = new();
    public bool IsUpdating { get; set; }
}
