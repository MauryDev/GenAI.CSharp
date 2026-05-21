using AdventoAPI.CPB.DTO;
using AdventoAPI.CPB.Utils; // LINQ normal
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
        using var semaphore = new SemaphoreSlim(5);
        var tarefas = blocos.SelectMany(bloco => bloco.Dias)
            .Select(async dia =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var info = await GetDevocional(dia.Href, cancellationToken);
                    if (info.Content != null && palavras.Any(palavra => info.Content.Contains(palavra, StringComparison.OrdinalIgnoreCase)))
                    {
                        return info;
                    }
                    return null;
                }
                finally
                {
                    semaphore.Release();
                }
            })
            .ToArray();



        return await Task.WhenEach(tarefas)
            .Select((e, i, token) => e.AsValueTask())
            .Where(r => r != null)
            .OfType<DevocionalInfo>()
            .ToListAsync(cancellationToken);



    }

    public async Task<List<DevocionalInfo>> BuscarPalavraChaveSemana(int semanaIndex, string palavra, CancellationToken cancellationToken = default)
    {
        return await BuscarPalavrasChaveSemana(semanaIndex, palavra.ToSingleIEnumerable(), cancellationToken);
    }

    public async Task<List<DevocionalInfo>> BuscarPalavrasChaveSemana(int semanaIndex, IEnumerable<string> palavras, CancellationToken cancellationToken = default)
    {
        var blocos = await GetDevocionaisAsync(cancellationToken);
        if (semanaIndex < 0 || semanaIndex >= blocos.Count)
        {
            return [];
        }
        using var semaphore = new SemaphoreSlim(5);


        var step1 = blocos.OrderBy(e => e.DataFinal)
            .ElementAt(semanaIndex).Dias
            .Select(async (DevocionalDiaInfo dia) =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await GetDevocional(dia.Href, cancellationToken);

                }
                catch (OperationCanceledException)
                {
                    return null;
                }
                finally
                {
                    semaphore.Release();
                }
            })
            .ToArray();


        return await Task.WhenEach(step1)
            .Select((e, i, token) => e.AsValueTask())
            .Where(info => info != null)
            .OfType<DevocionalInfo>()
            .ToListAsync(cancellationToken);



    }

    public async Task<List<DevocionalInfo>> BuscarPalavraChaveDataRange(DevocionalDayMonth dataInicio, DevocionalDayMonth dataFim, string palavra, CancellationToken cancellationToken = default)
    {
        return await BuscarPalavrasChaveDataRange(dataInicio, dataFim, palavra.ToSingleIEnumerable(), cancellationToken);
    }

    public async Task<List<DevocionalInfo>> BuscarPalavrasChaveDataRange(DevocionalDayMonth dataInicio, DevocionalDayMonth dataFim, IEnumerable<string> palavras, CancellationToken cancellationToken = default)
    {
        using var semaphore = new SemaphoreSlim(5);

        var blocos = await GetDevocionaisAsync(cancellationToken);

        var step1 = blocos
            .Where(bloco => bloco.DataFinal <= dataFim || bloco.DataInicio >= dataInicio)
            .SelectMany(bloco => bloco.Dias)
            .Where(dia => dia.Data >= dataInicio && dia.Data <= dataFim)
            .Select(async (DevocionalDiaInfo dia) =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await GetDevocional(dia.Href, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
                finally
                {
                    semaphore.Release();
                }
            })
            .ToArray();

        return await Task.WhenEach(step1)
            .Select((e, i, token) => e.AsValueTask())
            .Where(info => info != null && palavras.Any(palavra => info.Content != null && info.Content.Contains(palavra, StringComparison.OrdinalIgnoreCase)))
            .OfType<DevocionalInfo>()
            .ToListAsync(cancellationToken);

    }

    private async Task<IDocument> GetDocumentAsync(string url, CancellationToken cancellation = default)
    {

        var html = await _client.GetStringAsync(url, cancellation);
        var context = BrowsingContext.New(Configuration.Default);
        return await context.OpenAsync(req => req.Content(html), cancellation);
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

        if (string.IsNullOrWhiteSpace(dateText))
            throw new ArgumentException("Date text cannot be null or empty");

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
