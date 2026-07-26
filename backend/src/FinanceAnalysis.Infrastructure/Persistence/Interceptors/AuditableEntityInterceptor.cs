using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Domain.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinanceAnalysis.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Stamps <see cref="IAuditable.CreatedAt"/> and <see cref="IAuditable.UpdatedAt"/> on save,
/// so no service has to remember to do it and no entity can be written with a stale timestamp.
/// </summary>
public sealed class AuditableEntityInterceptor(IClock clock) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = clock.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    // CreatedAt is immutable once written.
                    entry.Property(nameof(IAuditable.CreatedAt)).IsModified = false;
                    break;

                default:
                    break;
            }
        }
    }
}
