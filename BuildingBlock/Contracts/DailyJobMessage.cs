namespace BuildingBlock.Contracts;

public class DailyJobMessage
{
    public string FreelancerId { get; set; } = default!;
    public string PlatformName { get; set; } = default!;
    public Guid TransactionID { get; set; }
}
