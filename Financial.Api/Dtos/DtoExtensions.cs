using Financial.Core.Models;

namespace Financial.Api.Controllers;

public static class DtoExtensions
{
    public static Movement ToDomain(this CreateMovementDto dto, Guid userId)
    {
        return new Movement(
            Guid.NewGuid(),
            userId,
            dto.Description,
            dto.Date,
            dto.Amount,
            dto.Category ?? "Other",
            dto.Type,
            dto.Tags ?? ""
        );
    }

    public static Movement ToDomain(this Movement movement, UpdateMovementDto dto)
    {
        movement.Description = dto.Description ?? movement.Description;
        movement.Date = dto.Date ?? movement.Date;
        movement.Amount = dto.Amount ?? movement.Amount;
        movement.Category = dto.Category ?? movement.Category;
        movement.Tags = dto.Tags ?? movement.Tags;
        movement.Type = dto.Type ?? movement.Type;
        
        return movement;
    }
}