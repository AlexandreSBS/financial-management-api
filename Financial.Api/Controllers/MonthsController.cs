using Financial.Core.Models;
using Financial.Data;
using Financial.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Financial.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonthsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public MonthsController(AppDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "Months endpoint is running" });
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<MonthlySummary>>> GetMonths()
    {
        try
        {
            var userId = _currentUserService.GetUserId();
            // Aggregate by year-month from Movements
            var movements = await _db.Movements.Where(m => m.UserId == userId).ToListAsync();

            var months = new Dictionary<string, MonthlySummary>();

            void EnsureMonth(DateTime d)
            {
                var key = d.ToString("yyyy-MM");
                if (!months.ContainsKey(key)) months[key] = new MonthlySummary { Month = key };
            }

            foreach (var movement in movements)
            {
                EnsureMonth(movement.Date);
                var key = movement.Date.ToString("yyyy-MM");

                if (movement.Type == MovementType.Expense)
                {
                    months[key].Expenses += movement.Amount;
                }
                else if (movement.Type == MovementType.Investment)
                {
                    months[key].Investments += movement.Amount;
                }
                else if (movement.Type == MovementType.Income)
                {
                    months[key].Income += movement.Amount;
                }
                else if (movement.Type == MovementType.EmergencyFund)
                {
                    months[key].EmergencyFundChange += movement.Amount;
                }
            }

            // Return ordered by month
            return Ok(months.Values.OrderBy(m => m.Month));
        }
        catch (UnauthorizedAccessException)
        {
            throw new ApplicationException("User not authenticated");
        }
    }
}
