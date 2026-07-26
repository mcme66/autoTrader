using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Domain.Catalog;

using Microsoft.EntityFrameworkCore;

namespace FinanceAnalysis.Infrastructure.Persistence.Repositories;

internal sealed class CompanyRepository(ApplicationDbContext db) : ICompanyRepository
{
    public Task<Company?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        db.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyDictionary<string, Company>> GetBySymbolsAsync(
        IReadOnlyCollection<string> symbols,
        CancellationToken cancellationToken = default)
    {
        if (symbols.Count == 0)
        {
            return new Dictionary<string, Company>(StringComparer.Ordinal);
        }

        var pairs = await db.Stocks
            .Where(s => symbols.Contains(s.Symbol))
            .Select(s => new { s.Symbol, s.Company })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return pairs.ToDictionary(p => p.Symbol, p => p.Company, StringComparer.Ordinal);
    }

    public void Add(Company company) => db.Companies.Add(company);
}
