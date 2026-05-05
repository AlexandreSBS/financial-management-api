namespace Financial.Core.Models;

public class User
{
    public Guid Id { get; set; }
    public string SubjectId { get; set; } = string.Empty; // From OAuth provider
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<Movement> Movements { get; set; } = new List<Movement>();
    public ICollection<EmergencyFund> EmergencyFunds { get; set; } = new List<EmergencyFund>();
}