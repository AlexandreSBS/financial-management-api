using Financial.Core.Models;
using Financial.Data;

namespace Financial.Core.Interfaces;

public interface IMovementRepository
{
    IUnitOfWork UnitOfWork { get; }

    Task<Movement?> GetMovementByIdAsync(
        Guid id,
        Guid userId,
        bool noTracking = false,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Movement>> GetMovementsByUserIdAsync(Guid userId, MovementType? type = null);
    Task<IEnumerable<Movement>> GetMovementsbyMonthsAsync(Guid userId, int month, int year, bool noTracking = true);
    void Add(Movement movement);
    void Update(Movement movement);
    void Delete(Movement movement);
}