namespace Financial.Data;

public interface IUnitOfWork
{
    Task<bool> CommitAsync();
}