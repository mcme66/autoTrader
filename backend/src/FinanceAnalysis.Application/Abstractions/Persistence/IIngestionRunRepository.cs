using FinanceAnalysis.Application.Common;
using FinanceAnalysis.Domain.Enums;
using FinanceAnalysis.Domain.MarketData;

namespace FinanceAnalysis.Application.Abstractions.Persistence;

public interface IIngestionRunRepository
{
    Task<IngestionRun?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<IngestionRun>> SearchAsync(
        PageRequest page,
        IngestionRunType? runType,
        IngestionRunStatus? status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// True when a run of this type for this trading day already completed successfully.
    /// Lets the daily endpoint be triggered more than once without doing duplicate work.
    /// </summary>
    Task<bool> HasSucceededForDateAsync(
        IngestionRunType runType,
        DateOnly tradeDate,
        CancellationToken cancellationToken = default);

    void Add(IngestionRun run);
}
