namespace FinanceAnalysis.Application.Common;

/// <summary>
/// Normalized pagination input. Clamping happens here rather than in each query so that a
/// hostile <c>pageSize=1000000</c> cannot reach the database.
/// </summary>
public readonly record struct PageRequest
{
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 200;

    public PageRequest(int? page, int? pageSize)
    {
        Page = page is null or < 1 ? 1 : page.Value;
        PageSize = pageSize switch
        {
            null or < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => pageSize.Value,
        };
    }

    public int Page { get; }

    public int PageSize { get; }

    public int Skip => (Page - 1) * PageSize;
}
