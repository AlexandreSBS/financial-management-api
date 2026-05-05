using System.ComponentModel.DataAnnotations;
using Financial.Core.Models;

namespace Financial.Api.Controllers;

public class UpdateMovementDto
{
    [StringLength(200, MinimumLength = 3)]
    public string? Description { get; set; }

    public DateTime? Date { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal? Amount { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    public MovementType? Type { get; set; }

    public string? Tags { get; set; }
}