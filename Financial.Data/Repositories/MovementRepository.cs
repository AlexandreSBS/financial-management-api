using Financial.Core.Interfaces;
using Financial.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Financial.Data.Repositories;

public class MovementRepository(AppDbContext context) : IMovementRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Movement?> GetMovementByIdAsync(
        Guid id,
        Guid userId,
        bool noTracking = false,
        CancellationToken cancellationToken = default)
    {
        var movement = context.Movements
            .Where(m => m.Id == id && m.UserId == userId);

        if (noTracking)
            movement.AsNoTracking();

        return await movement.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Movement>> GetMovementsByUserIdAsync(Guid userId, MovementType? type = null)
    {
        var query = context.Movements.Where(m => m.UserId == userId).AsQueryable();

        if (type.HasValue)
            query = query.Where(m => m.Type == type.Value);

        return await query.OrderByDescending(m => m.Date).ToListAsync();
    }

    public async Task<IEnumerable<Movement>> GetMovementsbyMonthsAsync(Guid userId, int month, int year,
        bool noTracking = true)
    {
        var movements = context.Movements
            .Where(m => m.UserId == userId && m.Date.Month == month && m.Date.Year == year);

        if (noTracking)
            movements.AsNoTracking();

        return await movements.ToListAsync();
    }

    public void Add(Movement movement)
    {
        context.Movements.Add(movement);
    }

    public void Update(Movement movement)
    {
        context.Movements.Update(movement);
    }

    public void Delete(Movement movement)
    {
        context.Movements.Remove(movement);
    }
}