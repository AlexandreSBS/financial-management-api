using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Financial.Core.Models;
using Financial.Data;
using Financial.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Financial.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovementsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public MovementsController(AppDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "API is running" });
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<Movement>>> GetMovements([FromQuery] MovementType? type = null)
    {
        try
        {
            var userId = _currentUserService.GetUserId();
            var query = _db.Movements.Where(m => m.UserId == userId).AsQueryable();
            
            if (type.HasValue)
            {
                query = query.Where(m => m.Type == type.Value);
            }
            
            var movements = await query.OrderByDescending(m => m.Date).ToListAsync();
            return Ok(movements);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var userId = _currentUserService.GetUserId();
            var movement = await _db.Movements.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (movement == null)
                return NotFound(new { message = "Movement not found" });

            return Ok(movement);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMovementDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new { 
                message = "Request body cannot be empty",
                code = "NULL_DTO"
            });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            return BadRequest(new { 
                message = "Validation failed",
                code = "VALIDATION_ERROR",
                errors = errors
            });
        }

        try
        {
            var userId = _currentUserService.GetUserId();
            var movement = new Movement(
                Guid.NewGuid(),
                userId,
                dto.Description,
                dto.Date,
                dto.Amount,
                dto.Category ?? "Other",
                dto.Type,
                dto.Tags ?? ""
            );
            
            _db.Movements.Add(movement);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = movement.Id }, movement);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                message = "Error creating movement",
                code = "CREATE_ERROR",
                details = ex.Message
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMovementDto dto)
    {
        try
        {
            var userId = _currentUserService.GetUserId();
            var movement = await _db.Movements.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (movement == null)
                return NotFound(new { message = "Movement not found" });

            movement.Description = dto.Description ?? movement.Description;
            movement.Date = dto.Date ?? movement.Date;
            movement.Amount = dto.Amount ?? movement.Amount;
            movement.Category = dto.Category ?? movement.Category;
            movement.Tags = dto.Tags ?? movement.Tags;
            movement.Type = dto.Type ?? movement.Type;

            _db.Movements.Update(movement);
            await _db.SaveChangesAsync();
            return Ok(movement);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = _currentUserService.GetUserId();
            var movement = await _db.Movements.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
            if (movement == null)
                return NotFound(new { message = "Movement not found" });

            _db.Movements.Remove(movement);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Movement deleted successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int month = 11, [FromQuery] int year = 2025)
    {
        try
        {
            var userId = _currentUserService.GetUserId();
            var movements = await _db.Movements
                .Where(m => m.UserId == userId && m.Date.Month == month && m.Date.Year == year)
                .ToListAsync();

            var summary = new
        {
            month = $"{year}-{month:D2}",
            income = movements.Where(m => m.Type == MovementType.Income).Sum(m => m.Amount),
            expenses = movements.Where(m => m.Type == MovementType.Expense).Sum(m => m.Amount),
            investments = movements.Where(m => m.Type == MovementType.Investment).Sum(m => m.Amount),
            total = movements.Sum(m => 
                m.Type == MovementType.Income ? m.Amount : -m.Amount)
        };

            return Ok(summary);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
    }
}

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
