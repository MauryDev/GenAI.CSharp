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
                var headerInfo = header != null ? ParseHeader(header!) : null;

                var dias = blocoElement.QuerySelectorAll(Options.DiasListaSelector)
                    .Select(diaElem => new DevocionalDiaInfo(
                        Data: ParseDateCPBStyle(diaElem.TextContent?.Trim()),
                        Titulo: diaElem.GetAttribute("title") ?? string.Empty,
                        Href: diaElem.GetAttribute("href") ?? string.Empty
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

    public async Task<DevocionalInfo> GetDevocional(string url, CancellationToken cancellation = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));


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

    public async Task<List<DevocionalInfo>> BuscarPalavraChaveDevocionais(string palavra, CancellationToken cancellationToken = default)
    {
        return await BuscarPalavrasChaveDevocionais(palavra.ToSingleIEnumerable(), cancellationToken);
    }

    public async Task<List<DevocionalInfo>> BuscarPalavrasChaveDevocionais(IEnumerable<string> palavras, CancellationToken cancellationToken = default)
    {
        var blocos = await GetDevocionaisAsync(cancellationToken);
        return await ProcessarDevocionaisAsync(blocos.SelectMany(b => b.Dias), palavras, cancellationToken);
    }

    public async Task<List<DevocionalInfo>> BuscarPalavraChaveSemana(int semanaIndex, string palavra, CancellationToken cancellationToken = default)
    {
        return await BuscarPalavrasChaveSemana(semanaIndex, palavra.ToSingleIEnumerable(), cancellationToken);
    }

    public async Task<List<DevocionalInfo>> BuscarPalavrasChaveSemana(int semanaIndex, IEnumerable<string> palavras, CancellationToken cancellationToken = default)
    {
        var blocos = await GetDevocionaisAsync(cancellationToken);
        if (semanaIndex < 0 || semanaIndex >= blocos.Count) return [];

        var dias = blocos.OrderBy(e => e.DataFinal).ElementAt(semanaIndex).Dias;
        return await ProcessarDevocionaisAsync(dias, palavras, cancellationToken);

    }

    public async Task<List<DevocionalInfo>> BuscarPalavraChaveDataRange(DevocionalDayMonth dataInicio, DevocionalDayMonth dataFim, string palavra, CancellationToken cancellationToken = default)
    {
        return await BuscarPalavrasChaveDataRange(dataInicio, dataFim, palavra.ToSingleIEnumerable(), cancellationToken);
    }

    public async Task<List<DevocionalInfo>> BuscarPalavrasChaveDataRange(DevocionalDayMonth dataInicio, DevocionalDayMonth dataFim, IEnumerable<string> palavras, CancellationToken cancellationToken = default)
    {
        var blocos = await GetDevocionaisAsync(cancellationToken);
        var dias = blocos.Where(b => b.DataFinal <= dataFim && b.DataInicio >= dataInicio)
                         .SelectMany(b => b.Dias)
                         .Where(d => d.Data >= dataInicio && d.Data <= dataFim);

        return await ProcessarDevocionaisAsync(dias, palavras, cancellationToken);
    }

    private async Task<IDocument> GetDocumentAsync(string url, CancellationToken cancellation = default)
    {

        var html = await _client.GetStringAsync(url, cancellation);
        return await _browsingContext.OpenAsync(req => req.Content(html), cancellation);
    }



    private async Task<List<DevocionalInfo>> ProcessarDevocionaisAsync(IEnumerable<DevocionalDiaInfo> dias, IEnumerable<string> palavras, CancellationToken cancellationToken)
    {
        using var semaphore = new SemaphoreSlim(5);

        var tarefas = dias.Select(async dia =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await GetDevocional(dia.Href, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToArray();

        return await Task.WhenEach(tarefas)
            .Select((e, i, token) => e.AsValueTask())
            .Where(info => info != null && palavras.Any(p => info.Content?.Contains(p, StringComparison.OrdinalIgnoreCase) ?? false))
            .OfType<DevocionalInfo>()
            .ToListAsync(cancellationToken);
    }


    private static MeditacoesHeader ParseHeader(string text)
    {
        var match = ParserHeaderRegex().Match(text);

        if (!match.Success)
            throw new FormatException($"Invalid header format: {text}");

        return new(
            DataInicio: ParseDateCPBStyle(match.Groups[1].Value),
            DataFinal: ParseDateCPBStyle(match.Groups[2].Value),
            NumberMeditacaoes: int.Parse(match.Groups[3].Value)
        );
    }

    public static DevocionalDayMonth ParseDateCPBStyle(string? dateText)
    {
        ArgumentException.ThrowIfNullOrEmpty(dateText, nameof(dateText));

        

        ReadOnlySpan<char> dateSpan = dateText.AsSpan();

        var enumerator = ParserDataRegex().EnumerateMatches(dateSpan);

        if (enumerator.MoveNext())
        {
            dateSpan = dateSpan[enumerator.Current.Length..];
        }

        if (!DateTime.TryParseExact(dateSpan, "d/MMM", CultureCustomPtBR.PtBrCulture, DateTimeStyles.None, out var parsedDate))
        {
            throw new FormatException($"Invalid date format: {dateText}");
        }

        return new DevocionalDayMonth(parsedDate.Day, parsedDate.Month);
    }

    [GeneratedRegex(@"(\d{1,2}/\w{3})\s*–\s*(\d{1,2}/\w{3})\s*\((\d+)\s*meditações\)")]
    private static partial Regex ParserHeaderRegex();
    [GeneratedRegex(@"^\w{3}\s+")]
    private static partial Regex ParserDataRegex();
}
