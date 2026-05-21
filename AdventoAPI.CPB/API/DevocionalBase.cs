using AdventoAPI.CPB.DTO;
using AdventoAPI.CPB.Utils; // LINQ normal
using AngleSharp;
using AngleSharp.Dom;
using System.Text.RegularExpressions;

namespace AdventoAPI.CPB.API;

public abstract partial class DevocionalBase(HttpClient? customClient = null)
{
    private static readonly HttpClient _sharedClient = new(new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),

        PooledConnectionLifetime = TimeSpan.FromMinutes(15)
    });
    public abstract string BaseUrl { get; }
    public abstract string MeditacoesUrl { get; }
    private readonly HttpClient _client = customClient ?? _sharedClient;

    public async Task<List<DevocionalSemanaBloco>> GetDevocionaisAsync(CancellationToken cancellation = default)
    {
        var document = await GetDocumentAsync(null, cancellation);

        return [.. document.QuerySelectorAll(".semana-bloco")
            .Select(blocoElement => new
            {
                Element = blocoElement,
                HeaderText = blocoElement.QuerySelector(".semana-header span")?.TextContent?.Trim()
            })
            .Where(bloco => !string.IsNullOrEmpty(bloco.HeaderText))
            .Select(bloco =>
            {
                var headerInfo = ParseHeader(bloco.HeaderText!);

                var dias = bloco.Element.QuerySelectorAll(".semana-body .dias-lista .dia-item a")
                    .Select(diaElem => new DevocionalDiaInfo(
                        Data: ParseDate(diaElem.TextContent?.Trim()),
                        Titulo: diaElem.GetAttribute("title") ?? string.Empty,
                        Href: diaElem.GetAttribute("href") ?? string.Empty
                    ))
                    .ToList();

                return new DevocionalSemanaBloco(
                    headerInfo.DataInicio,
                    headerInfo.DataFinal,
                    headerInfo.NumberMeditacaoes,
                    dias
                );
            })
        ];
    }

    

    public async Task<List<MeditacaoInfo>> GetMeditacaoInfoAsync(CancellationToken cancellation = default)
    {
        var document = await GetDocumentAsync(MeditacoesUrl, cancellation);
        var cards = document.QuerySelectorAll(".cpbCards");
        return [.. cards.Select(card => new MeditacaoInfo(
                Title : card.QuerySelector(".mediaCardTitle")?.TextContent?.Trim() ?? string.Empty,
                Description : card.QuerySelector(".mdl-card__supporting-text")?.TextContent?.Trim() ?? string.Empty
            ))
            .Where(cardData => !string.IsNullOrEmpty(cardData.Title))
        ];

       
    }

    public async Task<DevocionalInfo> GetDevocional(string url, CancellationToken cancellation = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));


        var document = await GetDocumentAsync(url, cancellation);

        string? diaNome = document.QuerySelector(".descriptionText.diaSemanaMeditacao")?.TextContent?.Trim()
            ?? "";

        string? diaMesNome = document.QuerySelector(".descriptionText.diaMesMeditacao")?.TextContent?.Trim()
            ?? "";

        string? versoBiblico = document.QuerySelector(".descriptionText.versoBiblico")?.TextContent?.Trim()
          ?? "";

        string? title = document.QuerySelector(".titleMeditacao")?.TextContent?.Trim();
        string? content = document.QuerySelector(".conteudoMeditacao")?.TextContent?.Trim();

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
        var val = await blocos.SelectMany(bloco => bloco.Dias)
            .ToAsyncEnumerable()
            .Select((DevocionalDiaInfo dia, CancellationToken cancellationToken) => GetDevocional(dia.Href, cancellationToken).AsValueTask())
            .Where(info => info.Content != null 
                && palavras.Any(palavra => info.Content.Contains(palavra, StringComparison.OrdinalIgnoreCase))
            )
            .ToListAsync(cancellationToken);
      
        return val;
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

        return await blocos.OrderBy(e => e.DataFinal)
            .ElementAt(semanaIndex)
            .Dias.ToAsyncEnumerable()
            .Select((DevocionalDiaInfo dia, CancellationToken cancellationToken) => GetDevocional(dia.Href, cancellationToken).AsValueTask())
            .Where(info => info.Content != null 
                && palavras.Any(palavra => info.Content.Contains(palavra, StringComparison.OrdinalIgnoreCase))
            )
            .ToListAsync(cancellationToken);

        
    }

    public async Task<List<DevocionalInfo>> BuscarPalavraChaveDataRange(DevocionalDayMonth dataInicio, DevocionalDayMonth dataFim, string palavra, CancellationToken cancellationToken = default)
    {
        return await BuscarPalavrasChaveDataRange(dataInicio,dataFim, palavra.ToSingleIEnumerable(), cancellationToken);
    }

    public async Task<List<DevocionalInfo>> BuscarPalavrasChaveDataRange(DevocionalDayMonth dataInicio, DevocionalDayMonth dataFim, IEnumerable<string> palavras, CancellationToken cancellationToken = default)
    {
        var blocos = await GetDevocionaisAsync(cancellationToken);
        return await blocos
            .Where(bloco => bloco.DataFinal <= dataFim || bloco.DataInicio >= dataInicio)
            .SelectMany(bloco => bloco.Dias)
            .Where(dia => dia.Data >= dataInicio && dia.Data <= dataFim)
            .ToAsyncEnumerable()
            .Select((DevocionalDiaInfo dia, CancellationToken cancelToken) =>  GetDevocional(dia.Href, cancelToken).AsValueTask())
            .Where(info => info?.Content != null && palavras.Any(palavra => info.Content.Contains(palavra, StringComparison.OrdinalIgnoreCase)))
            .ToListAsync(cancellationToken);
    }

    private async Task<IDocument> GetDocumentAsync(string? url = null, CancellationToken cancellation = default)
    {
        
        var html = await _client.GetStringAsync(url ?? BaseUrl, cancellation);
        var context = BrowsingContext.New(Configuration.Default);
        return await context.OpenAsync(req => req.Content(html), cancellation);
    }






    private static MeditacoesHeader ParseHeader(string text)
    {
        var match = ParserHeaderRegex().Match(text);

        if (!match.Success)
            throw new FormatException($"Invalid header format: {text}");

        return new(
            DataInicio: ParseDate(match.Groups[1].Value),
            DataFinal: ParseDate(match.Groups[2].Value),
            NumberMeditacaoes: int.Parse(match.Groups[3].Value)
        );
    }

    private static DevocionalDayMonth ParseDate(string? dateText)
    {
        
        if (string.IsNullOrWhiteSpace(dateText))
            throw new ArgumentException("Date text cannot be null or empty");

        var cleanDate = ParserDataRegex().Replace(dateText, "");

        var parts = cleanDate.Split('/');
        if (parts.Length != 2)
            throw new FormatException($"Invalid date format: {dateText}");

        int day = int.Parse(parts[0]);
        string monthAbbr = parts[1].ToLower();

        int month = monthAbbr switch
        {
            "jan" => 1,
            "fev" => 2,
            "mar" => 3,
            "abr" => 4,
            "mai" => 5,
            "jun" => 6,
            "jul" => 7,
            "ago" => 8,
            "set" => 9,
            "out" => 10,
            "nov" => 11,
            "dez" => 12,
            _ => throw new FormatException($"Unknown month abbreviation: {monthAbbr}")
        };

        return new DevocionalDayMonth(month, day);
    }

    [GeneratedRegex(@"(\d{1,2}/\w{3})\s*–\s*(\d{1,2}/\w{3})\s*\((\d+)\s*meditações\)")]
    private static partial Regex ParserHeaderRegex();
    [GeneratedRegex(@"^\w{3}\s+")]
    private static partial Regex ParserDataRegex();
}
