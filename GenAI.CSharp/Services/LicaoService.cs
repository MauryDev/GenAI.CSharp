using AngleSharp;
using AngleSharp.Dom;
using GenAI.CSharp.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace GenAI.CSharp.Services;


public class LicaoService : ISkillsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    ISkills ISkillsService.Skills => null;
    private const string Url =
        "https://mais.cpb.com.br/licao/a-oracao-na-pratica-2o-trimestre-2026/";

    private static readonly string[] DiasSemanaIds =
    {
                    "licaoDomingo", "licaoSegunda", "licaoTerca", "licaoQuarta", "licaoQuinta", "licaoSexta", "licaoSabado"
                };

    public LicaoService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /*
    |--------------------------------------------------------------------------
    | Public Methods
    |--------------------------------------------------------------------------
    */

    public async Task<object> GetSabado()
    {
        var licaosemana = await GetLicaoSemanaCore();
        return await GetSabadoInternal(licaosemana);
    }

    private async Task<object> GetSabadoInternal(LicaoSemana licaosemana)
    {
        var document = await GetDocumentAsync(licaosemana.Link);
        var sabado = document.QuerySelector("div#licaoSabado");

        var conteudo = sabado?.QuerySelector(".conteudoLicaoDia")?.TextContent?.Trim();
        var titulo = sabado?.QuerySelector(".titleLicao").TextContent?.Trim();
        var versoMemorizar = sabado?.QuerySelector(".versoMemorizar")?.TextContent?.Trim();

        return new
        {
            Dia = GetDiaSemana(6),
            Titulo = titulo,
            Conteudo = conteudo,
            VersoParaMemorizar = versoMemorizar
        };
    }

    public async Task<object> GetLicaoByDia(int dia)
    {
        var licaosemana = await GetLicaoSemanaCore();
        return await GetLicaoByWeekAndDiaInternal(licaosemana, dia);
    }

    public async Task<object> GetLicaoByWeekAndDia(int weekIndex, int dia)
    {
        var licaosemana = await GetLicaoSemanaByIndex(weekIndex);
        return await GetLicaoByWeekAndDiaInternal(licaosemana, dia);
    }

    private async Task<object> GetLicaoByWeekAndDiaInternal(LicaoSemana licaosemana, int dia)
    {
        if (dia == 6)
        {
            return await GetSabadoInternal(licaosemana);
        }

        var document = await GetDocumentAsync(licaosemana.Link);
        var elemento = document.QuerySelector($"div#{DiasSemanaIds[dia]}");
        var (titulo, conteudo) = ExtractLicaoInfo(elemento);

        return new
        {
            Dia = GetDiaSemana(dia),
            Titulo = titulo,
            Conteudo = conteudo
        };
    }

    public async Task<object> GetDiaAtual()
    {
        var licaosemana = await GetLicaoSemanaCore();
        var document = await GetDocumentAsync(licaosemana.Link);
        var selecionadoDia = document.QuerySelector("a.is-active");
        var href = selecionadoDia?.GetAttribute("href");

        if (string.IsNullOrWhiteSpace(href))
        {
            return new { Error = "Dia atual não encontrado." };
        }

        var id = href.TrimStart('#');
        var index = Array.IndexOf(DiasSemanaIds, id);

        var elemento = document.QuerySelector($"div{href}");
        var (titulo, conteudo) = ExtractLicaoInfo(elemento);

        return new
        {
            Dia = index != -1 ? GetDiaSemana(index) : "Não identificado",
            Titulo = titulo,
            Conteudo = conteudo,
        };
    }

    public async Task<object> GetTodosTemas()
    {
        var licaosemana = await GetLicaoSemanaCore();
        return await GetWeekTodosTemasInternal(licaosemana);
    }

    public async Task<object> GetWeekTodosTemas(int weekIndex)
    {
        var licaosemana = await GetLicaoSemanaByIndex(weekIndex);
        return await GetWeekTodosTemasInternal(licaosemana);
    }

    private async Task<object> GetWeekTodosTemasInternal(LicaoSemana licaosemana)
    {
        var document = await GetDocumentAsync(licaosemana.Link);
        var temas = document.QuerySelectorAll(".titleLicaoDay")
            .Select((e, index) => new
            {
                Dia = GetDiaSemana(index),
                Tema = e.TextContent?.Trim()
            });

        temas = temas.Append(new
        {
            Dia = GetDiaSemana(6),
            Tema = document.QuerySelector(".titleLicao")?.Text()
        });


        return new { Temas = temas };
    }

    public async Task<object> GetVersoMemorizar()
    {
        var licaosemana = await GetLicaoSemanaCore();
        return await GetWeekVersoMemorizarInternal(licaosemana);
    }

    public async Task<object> GetWeekVersoMemorizar(int weekIndex)
    {
        var licaosemana = await GetLicaoSemanaByIndex(weekIndex);
        return await GetWeekVersoMemorizarInternal(licaosemana);
    }

    private async Task<object> GetWeekVersoMemorizarInternal(LicaoSemana licaosemana)
    {
        var document = await GetDocumentAsync(licaosemana.Link);
        var verso = document.QuerySelector("div#licaoSabado .versoMemorizar")?.TextContent?.Trim();
        return new { Verso = verso };
    }

    public async Task<object> BuscarPalavraChave(string palavra)
    {
        var licaosemana = await GetLicaoSemanaCore();
        return await GetWeekBuscarPalavraChaveInternal(licaosemana, palavra);
    }

    public async Task<object> GetWeekBuscarPalavraChave(int weekIndex, string palavra)
    {
        var licaosemana = await GetLicaoSemanaByIndex(weekIndex);
        return await GetWeekBuscarPalavraChaveInternal(licaosemana, palavra);
    }

    private async Task<object> GetWeekBuscarPalavraChaveInternal(LicaoSemana licaosemana, string palavra)
    {
        var document = await GetDocumentAsync(licaosemana.Link);
        var resultados = new List<object>();

        for (int i = 0; i < DiasSemanaIds.Length; i++)
        {
            var id = DiasSemanaIds[i];
            var elemento = document.QuerySelector($"div#{id}");
            var (titulo, conteudo) = ExtractLicaoInfo(elemento);

            if (conteudo != null && conteudo.Contains(palavra, StringComparison.OrdinalIgnoreCase))
            {
                resultados.Add(new { Dia = GetDiaSemana(i), Titulo = titulo, Conteudo = conteudo });
            }
        }

        return new { Resultados = resultados };
    }

    public async Task<LicaoSemanaResumo> GetLicaoSemana()
    {
        const string licaoSemanaUrl = "https://mais.cpb.com.br/licao-adultos/";
        var document = await GetDocumentAsync(licaoSemanaUrl);
        var licaoCorrente = document.QuerySelector("licao-corrente")?.TextContent?.Trim();

        return JsonSerializer.Deserialize<LicaoSemanaResumo[]>(licaoCorrente)[0];
    }
    public async Task<LicaoSemanaResumo[]> GetLicoesTrimestre()
    {
        const string licaoSemanaUrl = "https://mais.cpb.com.br/licao-adultos/";
        var document = await GetDocumentAsync(licaoSemanaUrl);
        var licaoCorrente = document.QuerySelectorAll(".licoes")[1]?.TextContent?.Trim();

        return JsonSerializer.Deserialize<LicaoSemanaResumo[]>(licaoCorrente);
    }

    public async Task<object> GetLicaoByTrimestreIndex(int index)
    {
        var licoes = await GetLicoesTrimestre();
        if (licoes == null || index < 1 || index > licoes.Length)
        {
            return new { Error = $"Lição {index} não encontrada no trimestre. O trimestre possui {licoes?.Length ?? 0} lições." };
        }

        return licoes[index - 1];
    }

    public async Task<LicaoSemana> GetLicaoSemanaCore()
    {
        const string licaoSemanaUrl = "https://mais.cpb.com.br/licao-adultos/";
        var document = await GetDocumentAsync(licaoSemanaUrl);
        var licaoCorrente = document.QuerySelector("licao-corrente")?.TextContent?.Trim();

        return JsonSerializer.Deserialize<LicaoSemana[]>(licaoCorrente)[0];
    }

    public async Task<LicaoSemana> GetLicaoSemanaByIndex(int index)
    {
        const string licaoSemanaUrl = "https://mais.cpb.com.br/licao-adultos/";
        var document = await GetDocumentAsync(licaoSemanaUrl);
        var licoesJson = document.QuerySelectorAll(".licoes")[1]?.TextContent?.Trim();
        var licoes = JsonSerializer.Deserialize<LicaoSemana[]>(licoesJson);

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
        // Nota: Como GetDocumentAsync é static, ela não teria acesso ao _httpClientFactory.
        // Para manter a consistência com DI, removi o modificador 'static'.
        return await GetDocumentInternalAsync(url);
    }

    private async Task<IDocument> GetDocumentInternalAsync(string? url = null)
    {
        var client = _httpClientFactory.CreateClient();
        var html = await client.GetStringAsync(url ?? Url);
        var context = BrowsingContext.New(Configuration.Default);
        return await context.OpenAsync(req => req.Content(html));
    }

    // Ajuste para called methods: GetDocumentAsync agora é instância

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
            _ => "Dia inválido"
        };
    }

    public async Task<object> BuscarPalavraChaveTrimestre(string palavra)
    {
        var licoes = await GetLicoesTrimestre();
        var todosResultados = new List<object>();

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
                    todosResultados.Add(new
                    {
                        Semana = i + 1,
                        Dia = GetDiaSemana(d),
                        Titulo = titulo,
                        Conteudo = conteudo
                    });
                }
            }
        }

        return new { Resultados = todosResultados };
    }

    public async Task<object> GetCurrentWeekLessonAuxiliar()
    {
        var licaoSemana = await GetLicaoSemanaCore();
        return await GetLessonAuxiliarInternal(licaoSemana);
    }

    public async Task<object> GetWeekLessonAuxiliar(int weekIndex)
    {
        var licaoSemana = await GetLicaoSemanaByIndex(weekIndex);
        return await GetLessonAuxiliarInternal(licaoSemana);
    }

    private async Task<object> GetLessonAuxiliarInternal(LicaoSemana licaoSemana)
    {
        var document = await GetDocumentAsync(licaoSemana.Link);
        var aux = document.QuerySelector("#licaoAuxiliar");

        if (aux == null)
        {
            return new { Error = "Lição auxiliar não encontrada." };
        }

        var number = aux.QuerySelector(".descriptionText .numberLicao .numberLicaoAuxiliar")?.TextContent?.Trim();
        var title = aux.QuerySelector(".titleLicao .titleLicaoAuxiliar")?.TextContent?.Trim();
        var content = aux.QuerySelector(".conteudoLicaoDia")?.TextContent?.Trim();

        return new
        {
            NumeroLicao = number,
            Titulo = title,
            Conteudo = content
        };
    }


    public async Task<object> GetCurrentWeekLessonInformativo()
    {
        var licaoSemana = await GetLicaoSemanaCore();
        return await GetLessonInformativoInternal(licaoSemana);
    }

    public async Task<object> GetWeekLessonInformativo(int weekIndex)
    {
        var licaoSemana = await GetLicaoSemanaByIndex(weekIndex);
        return await GetLessonInformativoInternal(licaoSemana);
    }

    private async Task<object> GetLessonInformativoInternal(LicaoSemana licaoSemana)
    {
        var document = await GetDocumentAsync(licaoSemana.Link);
        var info = document.QuerySelector("#licaoInformativo");

        if (info == null)
        {
            return new { Error = "Informativo não encontrado." };
        }

        var content = info.QuerySelector(".conteudoLicaoDia")?.TextContent?.Trim();

        return new
        {
            Conteudo = content
        };
    }
}
