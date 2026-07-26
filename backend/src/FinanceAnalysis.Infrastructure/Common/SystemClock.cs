using FinanceAnalysis.Application.Common;

namespace FinanceAnalysis.Infrastructure.Common;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
