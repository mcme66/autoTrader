using FinanceAnalysis.Domain.Identity;

namespace FinanceAnalysis.UnitTests;

public sealed class RoleNamesTests
{
    [Fact]
    public void All_ContainsAdministratorAndMember()
    {
        RoleNames.All.ShouldContain(RoleNames.Administrator);
        RoleNames.All.ShouldContain(RoleNames.Member);
        RoleNames.All.Count.ShouldBe(2);
    }
}
