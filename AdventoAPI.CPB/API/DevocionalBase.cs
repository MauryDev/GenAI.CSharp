using AdventoAPI.CPB.DTO;
using AngleSharp;
using AngleSharp.Dom;
using System.Text.RegularExpressions;

namespace AdventoAPI.CPB.API;

public abstract class DevocionalBase(IHttpClientFactory httpClientFactory)
{
    public abstract string BaseUrl { get; }
    public abstract string MeditacoesUrl { get; }
    
    public async Task<List<DevocionalSemanaBloco>> GetDevocionaisAsync()
    {
        var document = await GetDocumentAsync();
        var blocosElements = document.QuerySelectorAll(".semana-bloco");
        var resultados = new List<DevocionalSemanaBloco>();

        foreach (var blocoElement in blocosElements)
        {
            var headerText = blocoElement.QuerySelector(".semana-header span")?.TextContent?.Trim();
            if (string.IsNullOrEmpty(headerText)) continue;

            var headerInfo = ParseHeader(headerText);

            var diaElements = blocoElement.QuerySelectorAll(".semana-body .dias-lista .dia-item a");
            var dias = new List<DevocionalDiaInfo>();

            foreach (var diaElem in diaElements)
            {
                dias.Add(new DevocionalDiaInfo(
                    Data: ParseDate(diaElem.TextContent?.Trim()),
                    Titulo: diaElem.GetAttribute("title") ?? string.Empty,
                    Href: diaElem.GetAttribute("href") ?? string.Empty
                ));
            }

            resultados.Add(new DevocionalSemanaBloco(
                headerInfo.DataInicio,
                headerInfo.DataFinal,
                headerInfo.NumberMeditacaoes,
                dias
            ));
        }

        return resultados;
    }

    public async Task<List<MeditacaoInfo>> GetMeditacaoInfoAsync()
    {
        var document = await GetDocumentAsync(MeditacoesUrl);
        var cards = document.QuerySelectorAll(".cpbCards");
        var resultados = new List<MeditacaoInfo>();

        foreach (var card in cards)
        {
            var title = card.QuerySelector(".mediaCardTitle")?.TextContent?.Trim() ?? string.Empty;
            var description = card.QuerySelector(".mdl-card__supporting-text")?.TextContent?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(title))
            {
                resultados.Add(new MeditacaoInfo(title, description));
            }
        }

        return resultados;
    }

    // New: fetch a specific devotional page using the provided url
    public async Task<DevocionalInfo> GetDevocional(string url)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));


        var document = await GetDocumentAsync(url);

        // Selectors (try both descendant and combined-class forms)
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

    private async Task<IDocument> GetDocumentAsync(string? url = null)
    {
        var client = httpClientFactory.CreateClient();
        var html = await client.GetStringAsync(url ?? BaseUrl);
        var context = BrowsingContext.New(Configuration.Default);
        return await context.OpenAsync(req => req.Content(html));
    }

    private (DateOnly DataInicio, DateOnly DataFinal, int NumberMeditacaoes) ParseHeader(string text)
    {
        // Expected format: "17/mai – 23/mai (2 meditações)"
        var match = Regex.Match(text, @"(\d{1,2}/\w{3})\s*–\s*(\d{1,2}/\w{3})\s*\((\d+)\s*meditações\)");

        if (!match.Success)
            throw new FormatException($"Invalid header format: {text}");

        return (
            DataInicio: ParseDate(match.Groups[1].Value),
            DataFinal: ParseDate(match.Groups[2].Value),
            NumberMeditacaoes: int.Parse(match.Groups[3].Value)
        );
    }

    private DateOnly ParseDate(string? dateText)
    {
        if (string.IsNullOrWhiteSpace(dateText))
            throw new ArgumentException("Date text cannot be null or empty");

        // Remove day of week prefix if present (e.g., "Sáb 16/mai" -> "16/mai")
        var cleanDate = Regex.Replace(dateText, @"^\w{3}\s+", "");

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

        // Use current year as the URL is for the current year
        return new DateOnly(DateTime.Now.Year, month, day);
    }
}
