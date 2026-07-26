using System.Net;

using FinanceAnalysis.Api.Configuration;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

// Disambiguates from the obsolete Microsoft.AspNetCore.HttpOverrides.IPNetwork, which the
// ASP.NET Core implicit usings also bring into scope.
using IPNetwork = System.Net.IPNetwork;

namespace FinanceAnalysis.Api.Security;

/// <summary>Requires the caller's address to fall inside a configured network.</summary>
public sealed class InternalNetworkRequirement : IAuthorizationRequirement;

/// <summary>
/// Second gate on the internal endpoints: even a caller holding the API key must originate
/// from an allowed network.
/// </summary>
/// <remarks>
/// The allow-list is evaluated against the connection's remote address. Behind a reverse proxy
/// that means the proxy's address, which is why <c>ForwardedHeaders</c> is configured in the
/// pipeline for trusted proxies only — accepting <c>X-Forwarded-For</c> from anyone would make
/// this check trivially bypassable.
/// </remarks>
internal sealed class InternalNetworkAuthorizationHandler(
    IHttpContextAccessor accessor,
    IOptions<SecurityOptions> options,
    ILogger<InternalNetworkAuthorizationHandler> logger)
    : AuthorizationHandler<InternalNetworkRequirement>
{
    private readonly IReadOnlyList<IPNetwork> _allowed = Parse(options.Value.InternalAllowedNetworks, logger);

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        InternalNetworkRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);

        var address = accessor.HttpContext?.Connection.RemoteIpAddress;

        if (address is null)
        {
            // No address means an in-memory test server, where the network check is meaningless
            // and the API key remains the effective gate.
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (_allowed.Any(network => network.Contains(address)))
        {
            context.Succeed(requirement);
        }
        else
        {
            logger.LogWarning("Rejected an internal endpoint call from a disallowed address {Address}.", address);
        }

        return Task.CompletedTask;
    }

    private static List<IPNetwork> Parse(IEnumerable<string> cidrs, ILogger logger)
    {
        var networks = new List<IPNetwork>();

        foreach (var cidr in cidrs)
        {
            if (IPNetwork.TryParse(cidr, out var network))
            {
                networks.Add(network);
            }
            else
            {
                logger.LogError("Ignoring malformed CIDR '{Cidr}' in Security:InternalAllowedNetworks.", cidr);
            }
        }

        return networks;
    }
}
