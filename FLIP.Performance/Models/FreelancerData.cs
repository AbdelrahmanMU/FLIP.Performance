namespace FLIP.Performance.Models;

public class FreelancerData
{
    public int Id { get; set; }
    public string PlatformName { get; set; } = default!;
    public Guid TransactionID { get; set; }
    public string NationalId { get; set; } = default!;
    public DateTime CreatedDate { get; set; }

    public string? JsonConvert { get; set; }
}
