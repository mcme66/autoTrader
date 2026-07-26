using FinanceAnalysis.Application.Abstractions.Persistence;

namespace FinanceAnalysis.Application.Features.Sectors;

/// <summary>A market sector, used to populate filters and the sector breakdown.</summary>
public sealed record SectorDto(string Key, string Name, int DisplayOrder);

public interface ISectorService
{
    Task<IReadOnlyList<SectorDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

public sealed class SectorService(ISectorRepository sectors) : ISectorService
{
    public async Task<IReadOnlyList<SectorDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var all = await sectors.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return [.. all.Select(s => new SectorDto(s.Key, s.Name, s.DisplayOrder))];
    }
}
