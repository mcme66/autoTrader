using System.Globalization;
using System.Text;

using FinanceAnalysis.Application.Abstractions.Persistence;
using FinanceAnalysis.Domain.MarketData;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using NpgsqlTypes;

namespace FinanceAnalysis.Infrastructure.Persistence.Repositories;

internal sealed class DailyPriceRepository(ApplicationDbContext db) : IDailyPriceRepository
{
    private const int ColumnsPerRow = 11;

    /// <summary>
    /// Postgres caps a statement at 65535 parameters. Batching well under that keeps a single
    /// oversized backfill from tripping the limit.
    /// </summary>
    private const int MaxRowsPerStatement = 500;

    public async Task<IReadOnlyList<DailyPrice>> GetRangeAsync(
        int stockId,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default) =>
        await db.DailyPrices
            .AsNoTracking()
            .Where(p => p.StockId == stockId && p.TradeDate >= fromDate && p.TradeDate <= toDate)
            .OrderBy(p => p.TradeDate)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public Task<DailyPrice?> GetLatestAsync(int stockId, CancellationToken cancellationToken = default) =>
        db.DailyPrices
            .AsNoTracking()
            .Where(p => p.StockId == stockId)
            .OrderByDescending(p => p.TradeDate)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<DateOnly?> GetLatestTradeDateAsync(CancellationToken cancellationToken = default)
    {
        var dates = await db.DailyPrices
            .AsNoTracking()
            .OrderByDescending(p => p.TradeDate)
            .Select(p => p.TradeDate)
            .Take(1)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return dates.Count == 0 ? null : dates[0];
    }

    /// <summary>
    /// Appends bars with <c>ON CONFLICT DO NOTHING</c> against the unique
    /// <c>(stock_id, trade_date)</c> index. This is the single point that enforces the
    /// "history is never overwritten" rule: a bar that already exists is skipped, never
    /// updated, so re-running an ingestion is idempotent and revisions from the provider
    /// cannot silently rewrite the past.
    /// </summary>
    public async Task<PriceInsertResult> InsertIgnoringDuplicatesAsync(
        IReadOnlyCollection<DailyPrice> prices,
        CancellationToken cancellationToken = default)
    {
        if (prices.Count == 0)
        {
            return new PriceInsertResult(0, 0);
        }

        var inserted = 0;

        foreach (var batch in prices.Chunk(MaxRowsPerStatement))
        {
            inserted += await InsertBatchAsync(batch, cancellationToken).ConfigureAwait(false);
        }

        return new PriceInsertResult(inserted, prices.Count - inserted);
    }

    private async Task<int> InsertBatchAsync(DailyPrice[] batch, CancellationToken cancellationToken)
    {
        var sql = new StringBuilder(
            """
            INSERT INTO daily_prices
                ("stock_id", "trade_date", "open", "high", "low", "close", "volume",
                 "volume_weighted_average_price", "transaction_count", "data_source_id", "ingested_at")
            VALUES
            """);

        var parameters = new List<NpgsqlParameter>(batch.Length * ColumnsPerRow);

        for (var row = 0; row < batch.Length; row++)
        {
            var offset = row * ColumnsPerRow;
            var price = batch[row];

            if (row > 0)
            {
                sql.Append(',');
            }

            sql.Append(CultureInfo.InvariantCulture, $" (@p{offset}");
            for (var column = 1; column < ColumnsPerRow; column++)
            {
                sql.Append(CultureInfo.InvariantCulture, $",@p{offset + column}");
            }

            sql.Append(')');

            parameters.Add(new NpgsqlParameter<int>($"p{offset}", price.StockId));
            parameters.Add(new NpgsqlParameter($"p{offset + 1}", NpgsqlDbType.Date) { Value = price.TradeDate });
            parameters.Add(new NpgsqlParameter<decimal>($"p{offset + 2}", price.Open));
            parameters.Add(new NpgsqlParameter<decimal>($"p{offset + 3}", price.High));
            parameters.Add(new NpgsqlParameter<decimal>($"p{offset + 4}", price.Low));
            parameters.Add(new NpgsqlParameter<decimal>($"p{offset + 5}", price.Close));
            parameters.Add(new NpgsqlParameter<long>($"p{offset + 6}", price.Volume));
            parameters.Add(Nullable($"p{offset + 7}", NpgsqlDbType.Numeric, price.VolumeWeightedAveragePrice));
            parameters.Add(Nullable($"p{offset + 8}", NpgsqlDbType.Integer, price.TransactionCount));
            parameters.Add(new NpgsqlParameter<int>($"p{offset + 9}", price.DataSourceId));
            parameters.Add(new NpgsqlParameter($"p{offset + 10}", NpgsqlDbType.TimestampTz)
            {
                Value = price.IngestedAt,
            });
        }

        sql.Append(" ON CONFLICT (\"stock_id\", \"trade_date\") DO NOTHING");

        return await db.Database
            .ExecuteSqlRawAsync(sql.ToString(), parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    private static NpgsqlParameter Nullable(string name, NpgsqlDbType type, object? value) =>
        new(name, type) { Value = value ?? DBNull.Value };
}
