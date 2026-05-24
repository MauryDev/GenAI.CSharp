using AdventoAPI.CPB.DTO;
using AngleSharp;
using AngleSharp.Dom;
using System.Text.Json;

namespace AdventoAPI.CPB.API;

public partial class LicaoAdulto(HttpClient? client = null, LicaoAdultoOptions? options = null)
{
    static readonly IBrowsingContext BrowsingContextInstance = BrowsingContext.New(Configuration.Default);

    private static readonly HttpClient SharedClient = new(new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
        PooledConnectionLifetime = TimeSpan.FromMinutes(15)
    });

    private readonly HttpClient _client = client ?? SharedClient;
    private readonly LicaoAdultoOptions _options = options ?? LicaoAdultoOptions.Default;

    public async Task<LicaoSemanaData?> GetLicaoAsync(Uri url, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentAsync(url, cancellationToken);
        return document.ParseSemana();
    }

    public async Task<LicaoSemanaAudiosResult?> GetLicaoSemanaAudiosAsync(Uri url, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentAsync(url, cancellationToken);
        return document.ParseSemanaAudios();
    }

    public Task<LicaoSemanaData?> GetLicaoAsync(string url, CancellationToken cancellationToken = default)
        => GetLicaoAsync(new Uri(url), cancellationToken);

    public Task<LicaoSemanaData?> GetLicaoAsync(LicaoSemanaItem licaoSemanaItem, CancellationToken cancellationToken = default)
        => GetLicaoAsync(licaoSemanaItem.Link, cancellationToken);

    public async Task<LicoesTrimestre> GetLicoesAsync(CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentAsync(_options.Urls.LicoesBaseUrl, cancellationToken);
        var baseElement = document.QuerySelector(_options.Selectors.CardsContainer);
        var licoesElements = baseElement?.QuerySelectorAll(_options.Selectors.LicoesList).ToList();

        if (licoesElements == null || licoesElements.Count < 2)
            return new LicoesTrimestre();

        var licaoCorrenteJson = licoesElements[0].TextContent;
        var licoesJson = licoesElements[1].TextContent;

        if (string.IsNullOrEmpty(licoesJson)) return new LicoesTrimestre();

        var dtoCurrentSemana = JsonSerializer.Deserialize<LicaoSemanaItemDTO[]>(licaoCorrenteJson)?[0];
        var dtoSemanas = JsonSerializer.Deserialize<LicaoSemanaItemDTO[]>(licoesJson)
            .Select(semana => new LicaoSemanaItem(semana.Img, semana.Title, semana.Verso, semana.Periodo.ObterLicaoSemanaPeriodo(), semana.Link))
            .ToArray();

        return new LicoesTrimestre
        {
            Trimestre = licoesElements[1]
                .GetAttribute("trimestre")
                .ExtrairNumeroTrimestre(),
            Ano = int.Parse(licoesElements[1].GetAttribute("ano")),
            CurrentSemana = new(dtoCurrentSemana.Img, dtoCurrentSemana.Title, dtoCurrentSemana.Verso, dtoCurrentSemana.Periodo.ObterLicaoSemanaPeriodo(), dtoCurrentSemana.Link),
            Semanas = dtoSemanas
        };
    }

    public async Task<List<LicaoSemanaData>> GetAllLicoesTrimestreAsync(CancellationToken cancellationToken = default)
    {
        var trimestre = await GetLicoesAsync(cancellationToken);

        if (trimestre.Semanas == null || trimestre.Semanas.Length == 0)
        {
            return [];
        }

        var tasks = trimestre.Semanas.Select(item => GetLicaoAsync(item, cancellationToken));

        var results = await Task.WhenAll(tasks);

        return [.. results.Where(r => r != null)
            .Cast<LicaoSemanaData>()];
    }

    public async Task<List<LicaoSemanaAudiosResult>> GetAllLicoesSemanaAudiosTrimestreAsync(CancellationToken cancellationToken = default)
    {
        var trimestre = await GetLicoesAsync(cancellationToken);

        if (trimestre.Semanas == null || trimestre.Semanas.Length == 0)
        {
            return [];
        }

        var tasks = trimestre.Semanas
            .Select(item => GetLicaoSemanaAudiosAsync(item.Link, cancellationToken));

        var results = await Task.WhenAll(tasks);

        return [.. results.Where(r => r != null)
            .Cast<LicaoSemanaAudiosResult>()
        ];
    }

    public async Task<LicaoAudios[]> GetLicoesAudiosTrimestreAsync(CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentAsync(_options.Urls.AudiosBaseUrl, cancellationToken);
        var cards = document.QuerySelectorAll(_options.Selectors.AudioCards);

        return [.. cards.Select(card => new LicaoAudios(
            CardMedia : card.QuerySelector(_options.Selectors.AudioCardMedia)?.GetAttribute("style").ExtractUrlFromStyle(),
            Title : card.QuerySelector(_options.Selectors.AudioCardTitle)?.TextContent?.Trim(),
            VersoMemorizar: card.QuerySelector(_options.Selectors.AudioCardVerso)?.TextContent?.Trim(),
            AudiosLink: card.QuerySelector(_options.Selectors.AudioCardLink)?.GetAttribute("href").TryParseUri()
        ))];
    }

    internal Task<IDocument> GetDocumentAsync(string url, CancellationToken cancellationToken = default)
        => GetDocumentAsync(new Uri(url), cancellationToken);

    internal async Task<IDocument> GetDocumentAsync(Uri url, CancellationToken cancellationToken = default)
    {
        var html = await _client.GetStringAsync(url, cancellationToken);
        return await BrowsingContextInstance.OpenAsync(req => req.Content(html), cancellationToken);
    }

    public async Task<List<LicaoSemanaData>> GetAllLicoesTrimestreThrottledAsync(int maxConcurrency, CancellationToken cancellationToken = default)
    {
        var trimestre = await GetLicoesAsync(cancellationToken);

        if (trimestre.Semanas == null || trimestre.Semanas.Length == 0)
        {
            return [];
        }

        using var semaphore = new SemaphoreSlim(maxConcurrency);

        var tasks = trimestre.Semanas
            .Select(async item =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await GetLicaoAsync(item, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            });

        var results = await Task.WhenAll(tasks);

        return [.. results
            .Where(r => r != null)
            .Cast<LicaoSemanaData>()
        ];
    }

    public async Task<List<LicaoSemanaAudiosResult>> GetAllLicoesSemanaAudiosTrimestreThrottledAsync(int maxConcurrency, CancellationToken cancellationToken = default)
    {
        var trimestre = await GetLicoesAsync(cancellationToken);

        if (trimestre.Semanas == null || trimestre.Semanas.Length == 0)
        {
            return [];
        }

        using var semaphore = new SemaphoreSlim(maxConcurrency);

        var tasks = trimestre.Semanas
            .Select(async item =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await GetLicaoSemanaAudiosAsync(item.Link, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            });

        var results = await Task.WhenAll(tasks);

        return [.. results
            .Where(r => r != null)
            .Cast<LicaoSemanaAudiosResult>()];
    }
}