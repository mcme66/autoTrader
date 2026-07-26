namespace FinanceAnalysis.Application.Abstractions.Persistence;

/// <summary>
/// Commits the changes tracked across the repositories participating in a request.
/// Repositories stage work; only this type writes it.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="operation"/> inside a database transaction, committing on success
    /// and rolling back on any exception. Used where a single logical change spans multiple
    /// <see cref="SaveChangesAsync"/> calls.
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);
}
