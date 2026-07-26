namespace FinanceAnalysis.Api.Security;

/// <summary>The authorization policy guarding <c>/api/internal/*</c>.</summary>
public static class InternalEndpointPolicy
{
    public const string Name = "InternalOnly";
}
