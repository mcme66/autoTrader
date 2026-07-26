namespace FinanceAnalysis.Application.Common;

/// <summary>
/// A single page of results plus the metadata a client needs to render pagination.
/// </summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) =>
        new([.. Items.Select(selector)], Page, PageSize, TotalCount);
}

/// <summary>Factory helpers for <see cref="PagedResult{T}"/>.</summary>
public static class PagedResult
{
    public static PagedResult<T> Empty<T>(int page, int pageSize) => new([], page, pageSize, 0);
}
