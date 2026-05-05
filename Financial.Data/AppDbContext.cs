using Financial.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Financial.Data;

public class AppDbContext : DbContext, IUnitOfWork 
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Movement> Movements => Set<Movement>();
    public DbSet<EmergencyFund> EmergencyFunds => Set<EmergencyFund>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User relationships
        modelBuilder.Entity<User>()
            .HasMany(u => u.Movements)
            .WithOne(m => m.User)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.EmergencyFunds)
            .WithOne(ef => ef.User)
            .HasForeignKey(ef => ef.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Create test user
        var testUserId = Guid.NewGuid();
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = testUserId,
            SubjectId = "test-user-12345",
            Email = "test@example.com",
            FullName = "Test User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Seed Emergency Fund for test user
        modelBuilder.Entity<EmergencyFund>().HasData(new EmergencyFund { Id = Guid.NewGuid(), UserId = testUserId, Amount = 5000m });

        // Seed sample Movements for test user
        modelBuilder.Entity<Movement>().HasData(
            // Income
            new Movement { Id = Guid.NewGuid(), UserId = testUserId, Description = "Salary", Amount = 5000m, Date = new DateTime(2025, 10, 1), Category = "Salary", Type = MovementType.Income },
            new Movement { Id = Guid.NewGuid(), UserId = testUserId, Description = "Freelance Project", Amount = 800m, Date = new DateTime(2025, 10, 15), Category = "Freelance", Type = MovementType.Income },
            new Movement { Id = Guid.NewGuid(), UserId = testUserId, Description = "Salary", Amount = 5000m, Date = new DateTime(2025, 11, 1), Category = "Salary", Type = MovementType.Income },
            new Movement { Id = Guid.NewGuid(), UserId = testUserId, Description = "Freelance Project", Amount = 600m, Date = new DateTime(2025, 11, 15), Category = "Freelance", Type = MovementType.Income },
            
            // Expenses
            new Movement { Id = Guid.NewGuid(), UserId = testUserId, Description = "Rent", Amount = 1500m, Date = new DateTime(2025, 10, 5), Category = "Housing", Type = MovementType.Expense },
            new Movement { Id = Guid.NewGuid(), UserId = testUserId, Description = "Groceries", Amount = 350m, Date = new DateTime(2025, 10, 10), Category = "Food", Type = MovementType.Expense },
            new Movement { Id = Guid.NewGuid(), UserId = testUserId, Description = "Utilities", Amount = 200m, Date = new DateTime(2025, 10, 15), Category = "Utilities", Type = MovementType.Expense },
            new Movement { Id = Guid.NewGuid(), UserId = testUserId, Description = "Rent", Amount = 1500m, Date = new DateTime(2025, 11, 5), Category = "Housing", Type = MovementType.Expense },
            new Movement { Id = Guid.NewGuid(), UserId = testUserId, Description = "Groceries", Amount = 400m, Date = new DateTime(2025, 11, 10), Category = "Food", Type = MovementType.Expense },
            new Movement { Id = Guid.NewGuid(), UserId = testUserId, Description = "Utilities", Amount = 220m, Date = new DateTime(2025, 11, 15), Category = "Utilities", Type = MovementType.Expense },
            
            // Investments
            new Movement { Id = Guid.NewGuid(), UserId = testUserId, Description = "Stock Portfolio", Amount = 500m, Date = new DateTime(2025, 10, 20), Category = "Stocks", Type = MovementType.Investment },
            new Movement { Id = Guid.NewGuid(), UserId = testUserId, Description = "Bond Fund", Amount = 300m, Date = new DateTime(2025, 10, 25), Category = "Bonds", Type = MovementType.Investment },
            new Movement { Id = Guid.NewGuid(), UserId = testUserId, Description = "Stock Portfolio", Amount = 600m, Date = new DateTime(2025, 11, 20), Category = "Stocks", Type = MovementType.Investment },
            new Movement { Id = Guid.NewGuid(), UserId = testUserId, Description = "Bond Fund", Amount = 350m, Date = new DateTime(2025, 11, 25), Category = "Bonds", Type = MovementType.Investment }
        );
    }

    public async Task<bool> CommitAsync()
    {
        return await base.SaveChangesAsync() > 0;
    }
}


