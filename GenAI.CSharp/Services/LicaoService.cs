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
    ISkills ISkillsService.Skills => new LicaoSkills(this);
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
        if (dia == 6)
        {
            return await GetSabado();
        }

        var licaosemana = await GetLicaoSemanaCore();
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
        var document = await GetDocumentAsync(licaosemana.Link);
        var verso = document.QuerySelector("div#licaoSabado .versoMemorizar")?.TextContent?.Trim();
        return new { Verso = verso };
    }

    public async Task<object> BuscarPalavraChave(string palavra)
    {
        var licaosemana = await GetLicaoSemanaCore();
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

    public async Task<object> GetLicaoSemana()
    {
        const string licaoSemanaUrl = "https://mais.cpb.com.br/licao-adultos/";
        var document = await GetDocumentAsync(licaoSemanaUrl);
        var licaoCorrente = document.QuerySelector("licao-corrente")?.TextContent?.Trim();

        return JsonSerializer.Deserialize<LicaoSemanaResumo[]>(licaoCorrente)[0];
    }
    public async Task<LicaoSemana> GetLicaoSemanaCore()
    {
        const string licaoSemanaUrl = "https://mais.cpb.com.br/licao-adultos/";
        var document = await GetDocumentAsync(licaoSemanaUrl);
        var licaoCorrente = document.QuerySelector("licao-corrente")?.TextContent?.Trim();

        return JsonSerializer.Deserialize<LicaoSemana[]>(licaoCorrente)[0];
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

    public class LicaoSkills(LicaoService licaoService) : ISkills
    {
        LicaoService licaoService = licaoService;
        [Description("Obtém a lição do dia atual.")]
        public Task<object> licao_GetDiaAtual()
        {
            return licaoService.GetDiaAtual();
        }
        [Description("0=domingo até 6=sábado")]
        public Task<object> licao_getLicaoByDia([Description("Obtém a lição por um dia específico.")][Required] int index)
        {
            return licaoService.GetLicaoByDia(index);
        }
        [Description("Obtém a lista de todos os temas/títulos das lições da semana.")]
        public Task<object> licao_getTodosTemas()
        {
            return licaoService.GetTodosTemas();
        }
        [Description("Obtém o verso para memorizar da lição de sábado.")]
        public Task<object> licao_obterVersoMemorizar()
        {
            return licaoService.GetVersoMemorizar();
        }
        [Description("Busca por uma palavra-chave em todas as lições da semana.")]
        public Task<object> licao_buscarPalavraChave([Description("Palavra ou frase a ser buscada.")][Required] string palavra)
        {
            return licaoService.BuscarPalavraChave(palavra);
        }
        [Description("Obtém a lição da semana corrente.")]
        public Task<object> licao_getLicaoSemana()
        {
            return licaoService.GetLicaoSemana();
        }
    }
}
