namespace Domain.Entities.App;

public class AppUser
{
    public Guid Id { get; set; }
    public string AppId { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
}