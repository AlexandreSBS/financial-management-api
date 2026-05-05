using System.ComponentModel.DataAnnotations;
using Financial.Core.Models;

namespace Financial.Api.Controllers;

public class CreateMovementDto
{
    [Required(ErrorMessage = "Description is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Description must be between 3 and 200 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [Required(ErrorMessage = "Type is required")]
    public MovementType Type { get; set; }

    public string? Tags { get; set; }
}