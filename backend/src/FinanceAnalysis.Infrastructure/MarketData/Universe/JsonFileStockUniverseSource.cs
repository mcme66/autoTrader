using System.Text.Json;
using System.Text.Json.Serialization;

using FinanceAnalysis.Application.Abstractions.MarketData;
using FinanceAnalysis.Application.Configuration;
using FinanceAnalysis.Domain.Exceptions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FinanceAnalysis.Infrastructure.MarketData.Universe;

/// <summary>
/// Reads the tracked universe from a JSON file on disk.
/// </summary>
/// <remarks>
/// The file is read on every sync rather than cached, so an operator can edit it and trigger a
/// sync without restarting the application. It is mounted read-only into the container in
/// production, which is why the path is configurable.
/// </remarks>
internal sealed class JsonFileStockUniverseSource(
    IOptions<MarketDataOptions> options,
    IHostEnvironment environment) : IStockUniverseSource
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private readonly MarketDataOptions _options = options.Value;

    public string Description => ResolvePath();

    public async Task<StockUniverse> LoadAsync(CancellationToken cancellationToken = default)
    {
        var path = ResolvePath();

        if (!File.Exists(path))
        {
            throw new NotFoundException(
                $"The universe file was not found at '{path}'. Set MarketData:UniverseFilePath to its location.");
        }

        await using var stream = File.OpenRead(path);

        UniverseFile? file;
        try
        {
            file = await JsonSerializer
                .DeserializeAsync<UniverseFile>(stream, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (JsonException ex)
        {
            throw new BusinessRuleException($"The universe file at '{path}' is not valid JSON: {ex.Message}");
        }

        if (file?.Symbols is null)
        {
            throw new BusinessRuleException($"The universe file at '{path}' has no 'symbols' array.");
        }

        var entries = new List<UniverseEntry>(file.Symbols.Count);

        foreach (var symbol in file.Symbols)
        {
            if (string.IsNullOrWhiteSpace(symbol.Symbol) || string.IsNullOrWhiteSpace(symbol.Name))
            {
                throw new BusinessRuleException(
                    $"Every entry in '{path}' must have a non-empty 'symbol' and 'name'.");
            }

            entries.Add(new UniverseEntry(
                symbol.Symbol.Trim(),
                symbol.Name.Trim(),
                symbol.Sector?.Trim() ?? string.Empty,
                string.IsNullOrWhiteSpace(symbol.Industry) ? null : symbol.Industry.Trim(),
                string.IsNullOrWhiteSpace(symbol.Exchange) ? null : symbol.Exchange.Trim()));
        }

        return new StockUniverse(file.Version ?? "unversioned", entries);
    }

    private string ResolvePath()
    {
        var configured = _options.UniverseFilePath;

        return Path.IsPathRooted(configured)
            ? configured
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, configured));
    }

    private sealed record UniverseFile(
        [property: JsonPropertyName("version")] string? Version,
        [property: JsonPropertyName("symbols")] IReadOnlyList<UniverseFileEntry>? Symbols);

    private sealed record UniverseFileEntry(
        [property: JsonPropertyName("symbol")] string? Symbol,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("sector")] string? Sector,
        [property: JsonPropertyName("industry")] string? Industry,
        [property: JsonPropertyName("exchange")] string? Exchange);
}
