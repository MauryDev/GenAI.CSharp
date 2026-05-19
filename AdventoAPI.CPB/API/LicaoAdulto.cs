using AdventoAPI.CPB.DTO;
using AngleSharp;
using AngleSharp.Dom;
using System.Text.Json;

namespace AdventoAPI.CPB.API;

public class LicaoAdulto
{
    private readonly IHttpClientFactory _httpClientFactory;
    private const string Url =
        "https://mais.cpb.com.br/licao/a-oracao-na-pratica-2o-trimestre-2026/";

    private static readonly string[] DiasSemanaIds =
    [
        "licaoDomingo", "licaoSegunda", "licaoTerca", "licaoQuarta", "licaoQuinta", "licaoSexta", "licaoSabado"
    ];

    public LicaoAdulto(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /*
    |-------------------------------------------------------------------------- 
    | Public Methods
    |-------------------------------------------------------------------------- 
    */

    public async Task<LicaoAdultoDay> GetSabado()
    {
        var licaosemana = await GetLicaoSemanaCore();
        return await GetSabadoInternal(licaosemana);
    }

    private async Task<LicaoAdultoDay> GetSabadoInternal(LicaoAdultoSemana licaosemana)
    {
        var document = await GetDocumentAsync(licaosemana.Link);
        var sabado = document.QuerySelector("div#licaoSabado");

        if (sabado == null)
        {
            throw new InvalidOperationException("Conteúdo de sábado não encontrado.");
        }

        var conteudo = sabado.QuerySelector(".conteudoLicaoDia")?.TextContent?.Trim();
        var titulo = sabado.QuerySelector(".titleLicao")?.TextContent?.Trim();
        var versoMemorizar = sabado.QuerySelector(".versoMemorizar")?.TextContent?.Trim();

        return new LicaoAdultoDay
        (
            DiaSemana: GetDiaSemana(6),
            Titulo: titulo,
            Conteudo: conteudo,
            VersoParaMemorizar: versoMemorizar
        );
    }

    public async Task<LicaoAdultoDay> GetLicaoByDiaSemana(int diaSemana)
    {
        var licaosemana = await GetLicaoSemanaCore();
        return await GetLicaoByWeekAndDiaInternal(licaosemana, diaSemana);
    }

    public async Task<LicaoAdultoDay> GetLicaoByWeekAndDiaSemana(int weekIndex, int diaSemana)
    {
        var licaosemana = await GetLicaoSemanaByIndex(weekIndex);
        return await GetLicaoByWeekAndDiaInternal(licaosemana, diaSemana);
    }

    private async Task<LicaoAdultoDay> GetLicaoByWeekAndDiaInternal(LicaoAdultoSemana licaosemana, int diaSemana)
    {
        if (diaSemana == 6)
        {
            return await GetSabadoInternal(licaosemana);
        }

        var document = await GetDocumentAsync(licaosemana.Link);
        var elemento = document.QuerySelector($"div#{DiasSemanaIds[diaSemana]}");
        var (titulo, conteudo) = ExtractLicaoInfo(elemento);

        return new LicaoAdultoDay(
            DiaSemana : GetDiaSemana(diaSemana),
            Titulo : titulo,
            Conteudo : conteudo,
            VersoParaMemorizar: null

        );
    }

    public async Task<LicaoAdultoDay> GetDiaAtual()
    {
        var licaosemana = await GetLicaoSemanaCore();
        var document = await GetDocumentAsync(licaosemana.Link);
        var selecionadoDia = document.QuerySelector("a.is-active");
        var href = selecionadoDia?.GetAttribute("href");
        var versoMemorizar = selecionadoDia.QuerySelector(".versoMemorizar")?.TextContent?.Trim();

        if (string.IsNullOrWhiteSpace(href))
        {
            throw new Exception("Dia atual não encontrado.");
        }

        var id = href.TrimStart('#');
        var index = Array.IndexOf(DiasSemanaIds, id);

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

    public async Task<LicaoTemasResponse> GetTodosTemas()
    {
        var licaosemana = await GetLicaoSemanaCore();
        return await GetWeekTodosTemasInternal(licaosemana);
    }

    public async Task<LicaoTemasResponse> GetWeekTodosTemas(int weekIndex)
    {
        var licaosemana = await GetLicaoSemanaByIndex(weekIndex);
        return await GetWeekTodosTemasInternal(licaosemana);
    }

    private async Task<LicaoTemasResponse> GetWeekTodosTemasInternal(LicaoAdultoSemana licaosemana)
    {
        var document = await GetDocumentAsync(licaosemana.Link);
        var temas = document.QuerySelectorAll(".titleLicaoDay")
            .Select((e, index) => new LicaoTemaDTO(
                GetDiaSemana(index),
                e.TextContent?.Trim()
            ));

        temas = temas.Append(new LicaoTemaDTO(
            GetDiaSemana(6),
            document.QuerySelector(".titleLicao")?.Text()
        ));

        return new LicaoTemasResponse(temas);
    }

    public async Task<LicaoVersoMemorizarDTO> GetVersoMemorizar()
    {
        var licaosemana = await GetLicaoSemanaCore();
        return await GetWeekVersoMemorizarInternal(licaosemana);
    }

    public async Task<LicaoVersoMemorizarDTO> GetWeekVersoMemorizar(int weekIndex)
    {
        var licaosemana = await GetLicaoSemanaByIndex(weekIndex);
        return await GetWeekVersoMemorizarInternal(licaosemana);
    }

    private async Task<LicaoVersoMemorizarDTO> GetWeekVersoMemorizarInternal(LicaoAdultoSemana licaosemana)
    {
        var document = await GetDocumentAsync(licaosemana.Link);
        var verso = document.QuerySelector("div#licaoSabado .versoMemorizar")?.TextContent?.Trim();
        return new LicaoVersoMemorizarDTO(verso);
    }

    public async Task<LicaoBuscaResponse> BuscarPalavraChave(string palavra)
    {
        var licaosemana = await GetLicaoSemanaCore();
        return await GetWeekBuscarPalavraChaveInternal(licaosemana, palavra);
    }

    public async Task<LicaoBuscaResponse> GetWeekBuscarPalavraChave(int weekIndex, string palavra)
    {
        var licaosemana = await GetLicaoSemanaByIndex(weekIndex);
        return await GetWeekBuscarPalavraChaveInternal(licaosemana, palavra);
    }

    private async Task<LicaoBuscaResponse> GetWeekBuscarPalavraChaveInternal(LicaoAdultoSemana licaosemana, string palavra)
    {
        var document = await GetDocumentAsync(licaosemana.Link);
        var resultados = new List<LicaoBuscaResultado>();

        for (int i = 0; i < DiasSemanaIds.Length; i++)
        {
            var id = DiasSemanaIds[i];
            var elemento = document.QuerySelector($"div#{id}");
            var (titulo, conteudo) = ExtractLicaoInfo(elemento);

            if (conteudo != null && conteudo.Contains(palavra, StringComparison.OrdinalIgnoreCase))
            {
                resultados.Add(new LicaoBuscaResultado(GetDiaSemana(i), titulo, conteudo));
            }
        }

        return new LicaoBuscaResponse(resultados);
    }

    public async Task<LicaoAdultoSemanaResumo> GetLicaoSemana()
    {
        const string licaoSemanaUrl = "https://mais.cpb.com.br/licao-adultos/";
        var document = await GetDocumentAsync(licaoSemanaUrl);
        var licaoCorrente = document.QuerySelector("licao-corrente")?.TextContent?.Trim();

        return JsonSerializer.Deserialize<LicaoAdultoSemanaResumo[]>(licaoCorrente)[0];
    }
    public async Task<LicaoAdultoSemanaResumo[]> GetLicoesTrimestre()
    {
        const string licaoSemanaUrl = "https://mais.cpb.com.br/licao-adultos/";
        var document = await GetDocumentAsync(licaoSemanaUrl);
        var licaoCorrente = document.QuerySelectorAll(".licoes")[1]?.TextContent?.Trim();

        return JsonSerializer.Deserialize<LicaoAdultoSemanaResumo[]>(licaoCorrente);
    }

    public async Task<LicaoAdultoSemanaResumo> GetLicaoByTrimestreIndex(int index)
    {
        var licoes = await GetLicoesTrimestre();
        if (licoes == null || index < 1 || index > licoes.Length)
        {
            throw new Exception($"Lição {index} não encontrada no trimestre. O trimestre possui {licoes?.Length ?? 0} lições.");
        }

        return licoes[index - 1];
    }

    public async Task<LicaoAdultoSemana> GetLicaoSemanaCore()
    {
        const string licaoSemanaUrl = "https://mais.cpb.com.br/licao-adultos/";
        var document = await GetDocumentAsync(licaoSemanaUrl);
        var licaoCorrente = document.QuerySelector("licao-corrente")?.TextContent?.Trim();

        return JsonSerializer.Deserialize<LicaoAdultoSemana[]>(licaoCorrente)[0];
    }

    public async Task<LicaoAdultoSemana> GetLicaoSemanaByIndex(int index)
    {
        const string licaoSemanaUrl = "https://mais.cpb.com.br/licao-adultos/";
        var document = await GetDocumentAsync(licaoSemanaUrl);
        var licoesJson = document.QuerySelectorAll(".licoes")[1]?.TextContent?.Trim();
        var licoes = JsonSerializer.Deserialize<LicaoAdultoSemana[]>(licoesJson);

        if (licoes == null || index < 1 || index > licoes.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index), $"A lição {index} não existe. O trimestre possui {licoes?.Length ?? 0} lições.");
        }

        return licoes[index - 1];
    }
    /*
    |--------------------------------------------------------------------------
    | Helpers
    |--------------------------------------------------------------------------
    */

    private async Task<IDocument> GetDocumentAsync(string? url = null)
    {
        return await GetDocumentInternalAsync(url);
    }

    private async Task<IDocument> GetDocumentInternalAsync(string? url = null)
    {
        var client = _httpClientFactory.CreateClient();
        var html = await client.GetStringAsync(url ?? Url);
        var context = BrowsingContext.New(Configuration.Default);
        return await context.OpenAsync(req => req.Content(html));
    }


    private (string? Titulo, string? Conteudo) ExtractLicaoInfo(IElement? element)
    {
        var titulo = element?.QuerySelector(".titleLicaoDay")?.TextContent?.Trim();
        var conteudo = element?.QuerySelector(".conteudoLicaoDia")?.TextContent?.Trim();
        return (titulo, conteudo);
    }

    private string GetDiaSemana(int index)
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
    public async Task<LicaoBuscaTrimestreResponse> BuscarPalavraChaveTrimestre(string palavra)
    {
        var licoes = await GetLicoesTrimestre();
        var todosResultados = new List<LicaoBuscaTrimestreResultado>();

        for (int i = 0; i < licoes.Length; i++)
        {
            var licaoSemana = await GetLicaoSemanaByIndex(i + 1);
            var document = await GetDocumentAsync(licaoSemana.Link);

            for (int d = 0; d < DiasSemanaIds.Length; d++)
            {
                var id = DiasSemanaIds[d];
                var elemento = document.QuerySelector($"div#{id}");
                var (titulo, conteudo) = ExtractLicaoInfo(elemento);

                if (conteudo != null && conteudo.Contains(palavra, StringComparison.OrdinalIgnoreCase))
                {
                    todosResultados.Add(new LicaoBuscaTrimestreResultado(
                        i + 1,
                        GetDiaSemana(d),
                        titulo,
                        conteudo));
                }
            }
        }

        return new LicaoBuscaTrimestreResponse(todosResultados);
    }

    public async Task<LicaoAuxiliarDTO> GetCurrentWeekLessonAuxiliar()
    {
        var licaoSemana = await GetLicaoSemanaCore();
        return await GetLessonAuxiliarInternal(licaoSemana);
    }

    public async Task<LicaoAuxiliarDTO> GetWeekLessonAuxiliar(int weekIndex)
    {
        var licaoSemana = await GetLicaoSemanaByIndex(weekIndex);
        return await GetLessonAuxiliarInternal(licaoSemana);
    }

    private async Task<LicaoAuxiliarDTO> GetLessonAuxiliarInternal(LicaoAdultoSemana licaoSemana)
    {
        var document = await GetDocumentAsync(licaoSemana.Link);
        var aux = document.QuerySelector("#licaoAuxiliar");

        if (aux == null)
        {
            throw new Exception("Lição auxiliar não encontrada.");
        }

        var number = aux.QuerySelector(".descriptionText .numberLicao .numberLicaoAuxiliar")?.TextContent?.Trim();
        var title = aux.QuerySelector(".titleLicao .titleLicaoAuxiliar")?.TextContent?.Trim();
        var content = aux.QuerySelector(".conteudoLicaoDia")?.TextContent?.Trim();

        return new LicaoAuxiliarDTO(number, title, content);
    }


    public async Task<LicaoInformativoDTO> GetCurrentWeekLessonInformativo()
    {
        var licaoSemana = await GetLicaoSemanaCore();
        return await GetLessonInformativoInternal(licaoSemana);
    }

    public async Task<LicaoInformativoDTO> GetWeekLessonInformativo(int weekIndex)
    {
        var licaoSemana = await GetLicaoSemanaByIndex(weekIndex);
        return await GetLessonInformativoInternal(licaoSemana);
    }

    private async Task<LicaoInformativoDTO> GetLessonInformativoInternal(LicaoAdultoSemana licaoSemana)
    {
        var document = await GetDocumentAsync(licaoSemana.Link);
        var info = document.QuerySelector("#licaoInformativo");

        if (info == null)
        {
            throw new Exception("Informativo não encontrado.");
        }

        var content = info.QuerySelector(".conteudoLicaoDia")?.TextContent?.Trim();

        return new LicaoInformativoDTO(content);
    }
}
