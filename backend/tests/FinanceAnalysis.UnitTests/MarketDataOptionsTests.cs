using FinanceAnalysis.Application.Configuration;

namespace FinanceAnalysis.UnitTests;

public sealed class MarketDataOptionsTests
{
    [Fact]
    public void Defaults_PreferMockProviderAndReasonableLimits()
    {
        var options = new MarketDataOptions();

        options.Provider.ShouldBe("mock");
        options.MaxTrackedSymbols.ShouldBe(500);
        options.Polygon.RequestsPerMinute.ShouldBe(5);
        options.UniverseFilePath.ShouldBe("config/universe.json");
    }
}
