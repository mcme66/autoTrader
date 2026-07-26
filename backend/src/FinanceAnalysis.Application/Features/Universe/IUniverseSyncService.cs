namespace FinanceAnalysis.Application.Features.Universe;

public interface IUniverseSyncService
{
    /// <summary>
    /// Reconciles the configured universe source into <c>companies</c> and <c>stocks</c>.
    /// Safe to run repeatedly: it is a diff, not a rebuild.
    /// </summary>
    Task<UniverseSyncResult> SyncAsync(CancellationToken cancellationToken = default);
}
