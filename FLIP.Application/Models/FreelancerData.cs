namespace FLIP.Application.Models;

public class FreelancerData
{
    public Guid TransactionID { get; set; }
    public DateTimeOffset IngestedAt { get; set; } = DateTime.Now.AddDays(-1);
    public string PlatformName { get; set; } = default!;
    public string NationalId { get; set; } = default!;

    public string? JsonContent { get; set; }
}
