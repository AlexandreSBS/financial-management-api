using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Financial.Core.Models;
using Financial.Api.Services;
using Financial.Core.Interfaces;

namespace Financial.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovementsController : ControllerBase
{
    private readonly IMovementRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public MovementsController(IMovementRepository repository, ICurrentUserService currentUserService)
    {
        _repository = repository;
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

            return Ok(await _repository.GetMovementsByUserIdAsync(userId));
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
            var movement = await _repository.GetMovementByIdAsync(id, userId);

            if (movement is null)
                return NotFound(new { message = "Movement not found" });

            return Ok(movement);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMovementDto? dto)
    {
        if (ActionResult(dto, out var badRequest)) return badRequest!;
        
        try
        {
            var userId = _currentUserService.GetUserId();
            var movement = dto!.ToDomain(userId);
            
            _repository.Add(movement);
            await _repository.UnitOfWork.CommitAsync();
            
            return CreatedAtAction(nameof(GetById), new { id = movement.Id }, movement);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error creating movement",
                code = "CREATE_ERROR",
                details = ex.Message
            });
        }
    }

    // move it to native middlewares or whatever
    private bool ActionResult(CreateMovementDto? dto, out IActionResult? badRequest)
    {
        if (dto is null)
        {
            badRequest = BadRequest(new
            {
                message = "Request body cannot be empty",
                code = "NULL_DTO"
            });
            return true;
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            badRequest = BadRequest(new
            {
                message = "Validation failed",
                code = "VALIDATION_ERROR",
                errors = errors
            });
            return true;
        }
        badRequest = null;
        return false;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMovementDto dto)
    {
        try
        {
            var userId = _currentUserService.GetUserId();
            var movement = await _repository.GetMovementByIdAsync(id, userId,false);

            if (movement == null)
                return NotFound(new { message = "Movement not found" });

            _repository.Update(movement.ToDomain(dto));
            
            await _repository.UnitOfWork.CommitAsync();
            
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
            var movement = await _repository.GetMovementByIdAsync(id, userId);

            if (movement == null)
                return NotFound(new { message = "Movement not found" });

            _repository.Delete(movement);
            await  _repository.UnitOfWork.CommitAsync();
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
            var summary = await GetSummaryAsync(month, year, userId);

            return Ok(summary);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "User not authenticated" });
        }
    }

    private async Task<object> GetSummaryAsync(int month, int year, Guid userId)
    {
        var movements = await _repository.GetMovementsbyMonthsAsync(userId, month, year);

        var summary = new
        {
            month = $"{year}-{month:D2}",
            income = movements.Where(m => m.Type == MovementType.Income).Sum(m => m.Amount),
            expenses = movements.Where(m => m.Type == MovementType.Expense).Sum(m => m.Amount),
            investments = movements.Where(m => m.Type == MovementType.Investment).Sum(m => m.Amount),
            total = movements.Sum(m =>
                m.Type == MovementType.Income ? m.Amount : -m.Amount)
        };
        
        return summary;
    }
}