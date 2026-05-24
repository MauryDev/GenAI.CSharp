using AdventoAPI.CPB.DTO;
using AdventoAPI.CPB.Utils;
using AngleSharp;
using AngleSharp.Dom;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AdventoAPI.CPB.API;

public abstract partial class DevocionalBase(HttpClient? client = null, DevocionalOptions? options = null)
{
    private static readonly HttpClient _sharedClient = new(new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),

        PooledConnectionLifetime = TimeSpan.FromMinutes(15)
    });

    public DevocionalOptions Options { get; set; } = options ?? DevocionalOptions.Default;

    private readonly HttpClient _client = client ?? _sharedClient;

    static readonly IBrowsingContext _browsingContext = BrowsingContext.New(Configuration.Default);

    public async Task<List<DevocionalSemanaBloco>> GetDevocionaisAsync(CancellationToken cancellation = default)
    {
        var document = await GetDocumentAsync(Options.BaseUrl, cancellation);

        return [.. document.QuerySelectorAll(Options.SemanaBlocoSelector)
            .Select(blocoElement => {
                var header = blocoElement.QuerySelector(Options.HeaderSelector)?.TextContent?.Trim();
                var headerInfo = header?.ParseHeader();

                var dias = blocoElement.QuerySelectorAll(Options.DiasListaSelector)
                    .Select(diaElem => new DevocionalDiaInfo(
                        Data: diaElem.TextContent?.Trim().ParseDateCPBStyle(),
                        Titulo: diaElem.GetAttribute("title") ?? string.Empty,
                        Href: new Uri(diaElem.GetAttribute("href"))
                    ))
                    .ToList();

                return new DevocionalSemanaBloco(
                    headerInfo?.DataInicio,
                    headerInfo?.DataFinal,
                    headerInfo?.NumberMeditacaoes ?? 0,
                    dias
                );
            })
        ];
    }



    public async Task<List<MeditacaoInfo>> GetMeditacaoInfoAsync(CancellationToken cancellation = default)
    {
        var document = await GetDocumentAsync(Options.MeditacoesUrl, cancellation);
        var cards = document.QuerySelectorAll(Options.CardSelector);
        return [.. cards.Select(card => new MeditacaoInfo(
                Title : card.QuerySelector(Options.TitleSelector)?.TextContent?.Trim() ?? string.Empty,
                Description : card.QuerySelector(Options.DescriptionSelector)?.TextContent?.Trim() ?? string.Empty
            ))
            .Where(cardData => !string.IsNullOrEmpty(cardData.Title))
        ];


    }

    public async Task<DevocionalInfo> GetDevocionalDia(Uri url, CancellationToken cancellation = default)
    {
        ArgumentNullException.ThrowIfNull(url, nameof(url));


        var document = await GetDocumentAsync(url, cancellation);

        string? diaNome = document.QuerySelector(Options.DiaSemanaSelector)?.TextContent?.Trim()
            ?? "";

        string? diaMesNome = document.QuerySelector(Options.DiaMesSelector)?.TextContent?.Trim()
            ?? "";

        string? versoBiblico = document.QuerySelector(Options.VersoBiblicoSelector)?.TextContent?.Trim()
          ?? "";

        string? title = document.QuerySelector(Options.MeditacaoTitleSelector)?.TextContent?.Trim();
        string? content = document.QuerySelector(Options.ContentSelector)?.TextContent?.Trim();

        return new(
            Url: url,
            DiadaSemanaNome: diaNome,
            DiaMesNome: diaMesNome,
            Title: title,
            Content: content,
            versoBiblico: versoBiblico
        );
    }



    private Task<IDocument> GetDocumentAsync(string url, CancellationToken cancellation = default)
        => GetDocumentAsync(new Uri(url), cancellation);

    private async Task<IDocument> GetDocumentAsync(Uri url, CancellationToken cancellation = default)
    {

        var html = await _client.GetStringAsync(url, cancellation);
        return await _browsingContext.OpenAsync(req => req.Content(html), cancellation);
    }



    

   
}
