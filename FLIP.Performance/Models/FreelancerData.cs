namespace FLIP.Performance.Models;

public class FreelancerData
{
    public string PlatformName { get; set; } = default!;
    public Guid TransactionID { get; set; }
    public string NationalId { get; set; } = default!;
    public DateTime IntegeratedAt { get; set; }

    public string? JsonConvert { get; set; }
}
