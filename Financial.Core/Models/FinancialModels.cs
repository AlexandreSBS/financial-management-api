namespace Financial.Core.Models;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MovementType
{
    Income,
    Expense,
    Investment,
    EmergencyFund
}

public class Movement
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } // Foreign key
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public MovementType Type { get; set; }

    // Navigation property
    public User? User { get; set; }

    public Movement() { }

    public Movement(Guid id, Guid userId, string description, DateTime date, decimal amount, string category, MovementType type, string tags = "")
    {
        Id = id;
        UserId = userId;
        Description = description;
        Date = date;
        Amount = amount;
        Category = category;
        Type = type;
        Tags = tags;
    }
}

public class EmergencyFund
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } // Foreign key
    public decimal Amount { get; set; }

    // Navigation property
    public User? User { get; set; }
}

public class MonthlySummary
{
    public string Month { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Investments { get; set; }
    public decimal EmergencyFundChange { get; set; }
}
