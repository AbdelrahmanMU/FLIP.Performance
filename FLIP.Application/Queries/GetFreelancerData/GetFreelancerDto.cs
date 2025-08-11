namespace FLIP.Application.Queries.GetFreelancerData;

public class GetFreelancerDto
{
    public int ID { get; set; }
    public Guid TransactionID { get; set; }
    public string? JsonContent { get; set; }
    public DateTime IngestedAt { get; set; }
    public string? NationalId { get; set; }
    public string? PlatformName { get; set; }
    public string Source { get; set; } = default!;
}
