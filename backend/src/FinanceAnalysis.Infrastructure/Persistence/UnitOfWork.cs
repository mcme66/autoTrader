using FinanceAnalysis.Application.Abstractions.Persistence;

using Microsoft.EntityFrameworkCore;

namespace FinanceAnalysis.Infrastructure.Persistence;

internal sealed class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // Nested calls join the ambient transaction rather than opening a second one.
        if (dbContext.Database.CurrentTransaction is not null)
        {
            return await operation(cancellationToken).ConfigureAwait(false);
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async ct =>
        {
            await using var transaction = await dbContext.Database
                .BeginTransactionAsync(ct)
                .ConfigureAwait(false);

            var result = await operation(ct).ConfigureAwait(false);

            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            await transaction.CommitAsync(ct).ConfigureAwait(false);

            return result;
        }, cancellationToken).ConfigureAwait(false);
    }
}
