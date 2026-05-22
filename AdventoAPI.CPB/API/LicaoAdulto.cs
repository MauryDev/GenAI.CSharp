using AdventoAPI.CPB.DTO;
using AdventoAPI.CPB.Utils;
using AngleSharp;
using AngleSharp.Dom;
using System.Text.Json;

namespace AdventoAPI.CPB.API;

public class LicaoAdulto(HttpClient? client = null, LicaoAdultoOptions? options = null)
{
    private static readonly HttpClient _sharedClient = new(new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),

        PooledConnectionLifetime = TimeSpan.FromMinutes(15)
    });

    private readonly HttpClient _client = client ?? _sharedClient;
    private readonly LicaoAdultoOptions _options = options ?? LicaoAdultoOptions.Default;

    /*
    |-------------------------------------------------------------------------- 
    | Public Methods
    |-------------------------------------------------------------------------- 
    */

    public async Task<LicaoAdultoDay> GetSabado(CancellationToken cancellationToken = default)
    {
        var licaosemana = await GetLicaoSemana(cancellationToken);
        return await GetSabadoInternal(licaosemana, cancellationToken);
    }

    private async Task<LicaoAdultoDay> GetSabadoInternal(LicaoAdultoSemana licaosemana, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentAsync(licaosemana.Link, cancellationToken);
        var sabado = document.QuerySelector(_options.SabadoSelector) ?? throw new InvalidOperationException("Conteúdo de sábado não encontrado.");
        var conteudo = sabado.QuerySelector(_options.ConteudoLicaoDiaSelector)?.TextContent?.Trim();
        var titulo = sabado.QuerySelector(_options.TitleLicaoSelector)?.TextContent?.Trim();
        var versoMemorizar = sabado.QuerySelector(_options.VersoMemorizarSelector)?.TextContent?.Trim();

        return new LicaoAdultoDay
        (
            DiaSemana: GetDiaSemana(6),
            Titulo: titulo,
            Conteudo: conteudo,
            VersoParaMemorizar: versoMemorizar
        );
    }

    public async Task<LicaoAdultoDay> GetLicaoByDiaSemana(int diaSemana, CancellationToken cancellationToken = default)
    {
        var licaosemana = await GetLicaoSemana(cancellationToken);
        return await GetLicaoByWeekAndDiaInternal(licaosemana, diaSemana, cancellationToken);
    }

    public async Task<LicaoAdultoDay> GetLicaoByWeekAndDiaSemana(int weekIndex, int diaSemana, CancellationToken cancellationToken = default)
    {
        var licaosemana = await GetLicaoSemanaByIndex(weekIndex, cancellationToken);
        return await GetLicaoByWeekAndDiaInternal(licaosemana, diaSemana, cancellationToken);
    }

    private async Task<LicaoAdultoDay> GetLicaoByWeekAndDiaInternal(LicaoAdultoSemana licaosemana, int diaSemana, CancellationToken cancellationToken = default)
    {
        if (diaSemana == 6)
        {
            return await GetSabadoInternal(licaosemana, cancellationToken);
        }

        var document = await GetDocumentAsync(licaosemana.Link, cancellationToken);
        var elemento = document.QuerySelector($"div#{_options.DiasSemanaIds[diaSemana]}");
        var (titulo, conteudo) = ExtractLicaoInfo(elemento);

        return new LicaoAdultoDay(
            DiaSemana: GetDiaSemana(diaSemana),
            Titulo: titulo,
            Conteudo: conteudo,
            VersoParaMemorizar: null

        );
    }

    public async Task<LicaoAdultoDay> GetDiaAtual(CancellationToken cancellationToken = default)
    {
        var licaosemana = await GetLicaoSemana(cancellationToken);
        var document = await GetDocumentAsync(licaosemana.Link, cancellationToken);
        var selecionadoDia = document.QuerySelector(_options.ActiveDaySelector);
        var href = selecionadoDia?.GetAttribute("href");
        var versoMemorizar = selecionadoDia?.QuerySelector(_options.VersoMemorizarSelector)?.TextContent?.Trim();

        if (string.IsNullOrWhiteSpace(href))
        {
            throw new KeyNotFoundException("Dia atual não encontrado.");
        }

        var id = href.TrimStart('#');
        var index = Array.IndexOf(_options.DiasSemanaIds, id);

        var elemento = document.QuerySelector($"div{href}");
        var (titulo, conteudo) = ExtractLicaoInfo(elemento);

        return new LicaoAdultoDay
        (
            DiaSemana: index != -1 ? GetDiaSemana(index) : "Não identificado",
            Titulo: titulo,
            Conteudo: conteudo,
            VersoParaMemorizar: versoMemorizar
        );
    }

    public async Task<LicaoTemasResponse> GetTodosTemas(CancellationToken cancellationToken = default)
    {
        var licaosemana = await GetLicaoSemana(cancellationToken);
        return await GetWeekTodosTemasInternal(licaosemana, cancellationToken);
    }

    public async Task<LicaoTemasResponse> GetWeekTodosTemas(int weekIndex, CancellationToken cancellationToken = default)
    {
        var licaosemana = await GetLicaoSemanaByIndex(weekIndex, cancellationToken);
        return await GetWeekTodosTemasInternal(licaosemana, cancellationToken);
    }

    private async Task<LicaoTemasResponse> GetWeekTodosTemasInternal(LicaoAdultoSemana licaosemana, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentAsync(licaosemana.Link, cancellationToken);
        var temas = document.QuerySelectorAll(_options.TitleLicaoDaySelector)
            .Select((e, index) => new LicaoTemaDTO(
                GetDiaSemana(index),
                e.TextContent?.Trim()
            ))
            .Append(new LicaoTemaDTO(
                GetDiaSemana(6),
                document.QuerySelector(_options.TitleLicaoSelector)?.Text()
            )
        );

        return new LicaoTemasResponse(temas);
    }

    public async Task<LicaoVersoMemorizarDTO> GetVersoMemorizar(CancellationToken cancellationToken = default)
    {
        var licaosemana = await GetLicaoSemana(cancellationToken);
        return await GetWeekVersoMemorizarInternal(licaosemana, cancellationToken);
    }

    public async Task<LicaoVersoMemorizarDTO> GetWeekVersoMemorizar(int weekIndex, CancellationToken cancellationToken = default)
    {
        var licaosemana = await GetLicaoSemanaByIndex(weekIndex, cancellationToken);
        return await GetWeekVersoMemorizarInternal(licaosemana, cancellationToken);
    }

    private async Task<LicaoVersoMemorizarDTO> GetWeekVersoMemorizarInternal(LicaoAdultoSemana licaosemana, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentAsync(licaosemana.Link, cancellationToken);
        var verso = document.QuerySelector($"{_options.SabadoSelector} {_options.VersoMemorizarSelector}")?.TextContent?.Trim();
        return new LicaoVersoMemorizarDTO(verso);
    }

    public async Task<LicaoBuscaResponse> BuscarPalavraChave(string palavra, CancellationToken cancellationToken = default)
    {
        var licaosemana = await GetLicaoSemana(cancellationToken);
        return await GetWeekBuscarPalavraChaveInternal(licaosemana, palavra, cancellationToken);
    }

    public async Task<LicaoBuscaResponse> GetWeekBuscarPalavraChave(int weekIndex, string palavra, CancellationToken cancellationToken = default)
    {
        var licaosemana = await GetLicaoSemanaByIndex(weekIndex, cancellationToken);
        return await GetWeekBuscarPalavraChaveInternal(licaosemana, palavra, cancellationToken);
    }

    public async Task<LicaoBuscaResponse> GetWeekBuscarPalavrasChave(int weekIndex, IEnumerable<string> palavras, CancellationToken cancellationToken = default)
    {
        var licaosemana = await GetLicaoSemanaByIndex(weekIndex, cancellationToken);
        return await GetWeekBuscarPalavrasChaveInternal(licaosemana, palavras, cancellationToken);
    }

    private async Task<LicaoBuscaResponse> GetWeekBuscarPalavraChaveInternal(LicaoAdultoSemana licaosemana, string palavra, CancellationToken cancellationToken = default)
    {

        return await GetWeekBuscarPalavrasChaveInternal(licaosemana, palavra.ToSingleIEnumerable(), cancellationToken);
    }


    private async Task<LicaoBuscaResponse> GetWeekBuscarPalavrasChaveInternal(LicaoAdultoSemana licaosemana, IEnumerable<string> palavras, CancellationToken cancellationToken = default)
    {

        var document = await GetDocumentAsync(licaosemana.Link, cancellationToken);
        var resultados = ProcessarDocumentoLicao(document, palavras).ToList();
        return new LicaoBuscaResponse(resultados);
    }
    public async Task<LicaoAdultoSemana> GetLicaoSemana(CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentAsync(_options.BaseUrl, cancellationToken);
        var licaoCorrente = document.QuerySelector(_options.LicaoCorrenteSelector)?.TextContent?.Trim();

        return JsonSerializer.Deserialize<LicaoAdultoSemana[]>(licaoCorrente)[0];
    }
    public async Task<LicaoAdultoSemana[]> GetLicoesTrimestre(CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentAsync(_options.BaseUrl, cancellationToken);
        var licaoCorrente = document.QuerySelectorAll(_options.LicoesSelector)[1]?.TextContent?.Trim();

        return JsonSerializer.Deserialize<LicaoAdultoSemana[]>(licaoCorrente);
    }

    public async Task<LicaoAdultoSemana> GetLicaoSemanaByIndex(int index, CancellationToken cancellationToken = default)
    {


        return (await GetLicoesTrimestre(cancellationToken))[index - 1];
    }





    public async Task<LicaoBuscaTrimestreResponse> BuscarPalavraChaveTrimestre(string palavra, CancellationToken cancellationToken = default)
    {

        return await BuscarPalavrasChaveTrimestre(palavra.ToSingleIEnumerable(), cancellationToken);
    }

    public async Task<LicaoBuscaTrimestreResponse> BuscarPalavrasChaveTrimestre(IEnumerable<string> palavras, CancellationToken cancellationToken = default)
    {
        var licoes = await GetLicoesTrimestre(cancellationToken);

        var todosResultados = await licoes.ToAsyncEnumerable()
            .SelectMany(async (licao, i, ct) =>
            {
                var document = await GetDocumentAsync(licao.Link, ct);
                var numeroSemana = i + 1;

                return ProcessarDocumentoLicao(document, palavras)
                    .Select(res => new LicaoBuscaTrimestreResultado(numeroSemana, res.Dia, res.Titulo, res.Conteudo));
            })
            .ToListAsync(cancellationToken);

        return new LicaoBuscaTrimestreResponse(todosResultados);
    }

    private IEnumerable<LicaoBuscaResultado> ProcessarDocumentoLicao(IDocument document, IEnumerable<string> palavras)
    {
        return _options.DiasSemanaIds
            .Select((id, i) =>
            {
                var elemento = document.QuerySelector($"div#{id}");
                var (titulo, conteudo) = ExtractLicaoInfo(elemento);
                return new LicaoBuscaResultado(GetDiaSemana(i), titulo, conteudo);
            })
            .Where(info => info.Conteudo != null
                && palavras.Any(p => info.Conteudo.Contains(p, StringComparison.OrdinalIgnoreCase))
            );
    }


    public async Task<LicaoAuxiliarDTO> GetCurrentWeekLessonAuxiliar(CancellationToken cancellationToken = default)
    {
        var licaoSemana = await GetLicaoSemana(cancellationToken);
        return await GetLessonAuxiliarInternal(licaoSemana, cancellationToken);
    }

    public async Task<LicaoAuxiliarDTO> GetWeekLessonAuxiliar(int weekIndex, CancellationToken cancellationToken = default)
    {
        var licaoSemana = await GetLicaoSemanaByIndex(weekIndex, cancellationToken);
        return await GetLessonAuxiliarInternal(licaoSemana, cancellationToken);
    }

    private async Task<LicaoAuxiliarDTO> GetLessonAuxiliarInternal(LicaoAdultoSemana licaoSemana, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentAsync(licaoSemana.Link, cancellationToken);
        var aux = document.QuerySelector(_options.LicaoAuxiliarSelector) ?? throw new Exception("Lição auxiliar não encontrada.");
        var number = aux.QuerySelector(_options.LicaoAuxiliarNumberSelector)?.TextContent?.Trim();
        var title = aux.QuerySelector(_options.LicaoAuxiliarTitleSelector)?.TextContent?.Trim();
        var content = aux.QuerySelector(_options.ConteudoLicaoDiaSelector)?.TextContent?.Trim();

        return new LicaoAuxiliarDTO(number, title, content);
    }


    public async Task<LicaoInformativoDTO> GetCurrentWeekLessonInformativo(CancellationToken cancellationToken = default)
    {
        var licaoSemana = await GetLicaoSemana(cancellationToken);
        return await GetLessonInformativoInternal(licaoSemana, cancellationToken);
    }

    public async Task<LicaoInformativoDTO> GetWeekLessonInformativo(int weekIndex, CancellationToken cancellationToken = default)
    {
        var licaoSemana = await GetLicaoSemanaByIndex(weekIndex, cancellationToken);
        return await GetLessonInformativoInternal(licaoSemana, cancellationToken);
    }

    private async Task<LicaoInformativoDTO> GetLessonInformativoInternal(LicaoAdultoSemana licaoSemana, CancellationToken cancellationToken = default)
    {
        var document = await GetDocumentAsync(licaoSemana.Link, cancellationToken);
        var info = document.QuerySelector(_options.LicaoInformativoSelector) ?? throw new Exception("Informativo não encontrado.");
        var content = info.QuerySelector(_options.ConteudoLicaoDiaSelector)?.TextContent?.Trim();

        return new LicaoInformativoDTO(content);
    }


    /*
    |--------------------------------------------------------------------------
    | Helpers
    |--------------------------------------------------------------------------
    */

    private async Task<IDocument> GetDocumentAsync(string? url, CancellationToken cancellationToken = default)
    {
        return await GetDocumentInternalAsync(url, cancellationToken);
    }

    private async Task<IDocument> GetDocumentInternalAsync(string? url, CancellationToken cancellationToken = default)
    {
        var html = await _client.GetStringAsync(url, cancellationToken);
        var context = BrowsingContext.New(Configuration.Default);
        return await context.OpenAsync(req => req.Content(html), cancellationToken);
    }


    (string? Titulo, string? Conteudo) ExtractLicaoInfo(IElement? element)
    {
        var titulo = element?.QuerySelector(_options.TitleLicaoDaySelector)?.TextContent?.Trim();
        var conteudo = element?.QuerySelector(_options.ConteudoLicaoDiaSelector)?.TextContent?.Trim();
        return (titulo, conteudo);
    }

    private static string GetDiaSemana(int index)
    {
        return index switch
        {
            0 => "Domingo",
            1 => "Segunda-feira",
            2 => "Terça-feira",
            3 => "Quarta-feira",
            4 => "Quinta-feira",
            5 => "Sexta-feira",
            6 => "Sábado",
            _ => throw new ArgumentOutOfRangeException(nameof(index), "Dia da semana inválido.")
        };
    }

}
