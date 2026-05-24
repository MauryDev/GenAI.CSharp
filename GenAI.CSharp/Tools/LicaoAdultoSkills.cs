using AdventoAPI.CPB.API;
using AdventoAPI.CPB.DTO;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GenAI.CSharp.Tools;

/// <summary>
/// Define os métodos de ferramenta (MCP Tools) para acesso à Lição da Escola Sabatina Adulta,
/// adaptando os comportamentos desejados com base na nova implementação da LicaoAdulto.
/// </summary>
[McpServerToolType]
public class LicaoAdultoSkills
{
    private readonly LicaoAdulto _licaoService;

    public LicaoAdultoSkills(IHttpClientFactory httpClientFactory)
    {
        _licaoService = new LicaoAdulto(httpClientFactory.CreateClient());
    }

    [McpServerTool]
    [Description("Obtém a lição do dia atual da semana corrente.")]
    public async Task<object?> Licao_GetLicaoCorrenteDiaAtual()
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.CurrentSemana == null) return null;

        var licaoSemana = await _licaoService.GetLicaoAsync(licoes.CurrentSemana);
        if (licaoSemana == null) return null;

        // Identifica o dia atual da semana (0 = Domingo, ..., 6 = Sábado)
        var diaSemana = DateTime.Today.DayOfWeek;
        return ObterLicaoPorDiaSemana(licaoSemana, diaSemana);
    }

    [McpServerTool]
    [Description("Obtém a lição por um dia da semana específico da semana corrente (0=domingo até 6=sábado).")]
    public async Task<object?> Licao_GetSemanaLicaoCorrenteByDiaSemana(
        [Description("Índice do dia: 0=domingo até 6=sábado")][Required] int index)
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.CurrentSemana == null) return null;

        var licaoSemana = await _licaoService.GetLicaoAsync(licoes.CurrentSemana);
        if (licaoSemana == null) return null;

        var diaSemana = (DayOfWeek)index;
        return ObterLicaoPorDiaSemana(licaoSemana, diaSemana);
    }

    [McpServerTool]
    [Description("Obtém a lição de uma semana específica do trimestre por um dia específico.")]
    public async Task<object?> Licao_GetLicaoByWeekAndDia(
        [Description("Número da semana no trimestre (1-13, baseado em 1).")][Required] int weekIndex,
        [Description("Índice do dia: 0=domingo até 6=sábado")][Required] int diaIndex)
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.Semanas == null || weekIndex < 1 || weekIndex > licoes.Semanas.Length)
            return null;

        var semanaSelecionada = licoes.Semanas[weekIndex - 1];
        var licaoSemana = await _licaoService.GetLicaoAsync(semanaSelecionada);
        if (licaoSemana == null) return null;

        var diaSemana = (DayOfWeek)diaIndex;
        return ObterLicaoPorDiaSemana(licaoSemana, diaSemana);
    }
    [McpServerTool]
    [Description("Obtém a lista de todos os temas/títulos das lições da semana corrente.")]
    public async Task<object?> Licao_GetLicaoCorrenteTodosTemas()
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.CurrentSemana == null) return null;

        var licaoSemana = await _licaoService.GetLicaoAsync(licoes.CurrentSemana);
        if (licaoSemana == null) return null;

        return ExtrairTemasDaSemana(licaoSemana);
    }
    [McpServerTool]
    [Description("Obtém a lista de todos os temas/títulos das lições de uma semana específica.")]
    public async Task<object?> Licao_GetLicaoByWeekTodosTemas(
        [Description("Número da semana no trimestre (1-13).")][Required] int weekIndex)
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.Semanas == null || weekIndex < 1 || weekIndex > licoes.Semanas.Length)
            return null;

        var semanaSelecionada = licoes.Semanas[weekIndex - 1];
        var licaoSemana = await _licaoService.GetLicaoAsync(semanaSelecionada);
        if (licaoSemana == null) return null;

        return ExtrairTemasDaSemana(licaoSemana);
    }
    [McpServerTool]
    [Description("Obtém o verso para memorizar da lição de sábado da semana corrente.")]
    public async Task<object?> Licao_GetLicaoCorrenteVersoMemorizar()
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.CurrentSemana == null) return null;

        var licaoSemana = await _licaoService.GetLicaoAsync(licoes.CurrentSemana);
        return new { VersoMemorizar = licaoSemana?.Sabado?.VersoMemorizar };
    }
    [McpServerTool]
    [Description("Obtém o verso para memorizar da lição de sábado de uma semana específica.")]
    public async Task<object?> Licao_GetLicaoByWeekVersoMemorizar(
        [Description("Número da semana no trimestre (1-13).")][Required] int weekIndex)
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.Semanas == null || weekIndex < 1 || weekIndex > licoes.Semanas.Length)
            return null;

        var semanaSelecionada = licoes.Semanas[weekIndex - 1];
        var licaoSemana = await _licaoService.GetLicaoAsync(semanaSelecionada);
        return new { VersoMemorizar = licaoSemana?.Sabado?.VersoMemorizar };
    }

    [McpServerTool]
    [Description("Busca por uma palavra-chave em todas as lições da semana corrente.")]
    public async Task<object?> Licao_BuscarLicaoCorrentePalavraChave(
        [Description("Palavra ou frase a ser buscada na semana corrente.")][Required] string palavra)
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.CurrentSemana == null) return null;

        var licaoSemana = await _licaoService.GetLicaoAsync(licoes.CurrentSemana);
        if (licaoSemana == null) return null;

        return ExecutarBuscaPalavraChaveNaSemana(licaoSemana, palavra);
    }
    [McpServerTool]
    [Description("Busca por uma palavra-chave em todas as lições de uma semana específica.")]
    public async Task<object?> Licao_BuscarLicaoByWeekPalavraChave(
        [Description("Número da semana no trimestre (1-13).")][Required] int weekIndex,
        [Description("Palavra ou frase a ser buscada.")][Required] string palavra)
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.Semanas == null || weekIndex < 1 || weekIndex > licoes.Semanas.Length)
            return null;

        var semanaSelecionada = licoes.Semanas[weekIndex - 1];
        var licaoSemana = await _licaoService.GetLicaoAsync(semanaSelecionada);
        if (licaoSemana == null) return null;

        return ExecutarBuscaPalavraChaveNaSemana(licaoSemana, palavra);
    }
    [McpServerTool]
    [Description("Obtém os detalhes básicos (Título, Período e Verso) da lição da semana corrente.")]
    public async Task<object?> Licao_GetLicaoSemana()
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.CurrentSemana == null) return null;

        return new
        {
            Title = licoes.CurrentSemana.Title,
            Periodo = licoes.CurrentSemana.Periodo,
            Verso = licoes.CurrentSemana.Verso
        };
    }
    [McpServerTool]
    [Description("Obtém os detalhes básicos (Título, Período e Verso) de uma lição por número no trimestre.")]
    public async Task<object?> Licao_GetLicaoSemanaByIndex(
        [Description("Número da lição no trimestre (ex: 1, 2, 3).")][Required] int index)
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.Semanas == null || index < 1 || index > licoes.Semanas.Length)
            return null;

        var semana = licoes.Semanas[index - 1];
        return new
        {
            Title = semana.Title,
            Periodo = semana.Periodo,
            Verso = semana.Verso
        };
    }
    [McpServerTool]
    [Description("Obtém a lista de todas as lições (metadados básicos) do trimestre corrente.")]
    public async Task<object?> Licao_GetLicoesTrimestre()
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.Semanas == null) return Array.Empty<object>();

        return licoes.Semanas.Select(semana => new
        {
            Title = semana.Title,
            Periodo = semana.Periodo,
            Verso = semana.Verso
        }).ToArray();
    }
    [McpServerTool]
    [Description("Busca por uma palavra-chave em todo o trimestre atual.")]
    public async Task<object> Licao_BuscarTrimestreCompletoPalavraChave(
        [Description("Palavra ou frase a ser buscada em todo o trimestre.")][Required] string palavra)
    {
        var licoesCompletas = await _licaoService.GetAllLicoesTrimestreThrottledAsync(4);
        var resultados = new List<object>();

        for (int i = 0; i < licoesCompletas.Count; i++)
        {
            var licaoSemana = licoesCompletas[i];
            var ocorrencias = ExecutarBuscaPalavraChaveNaSemana(licaoSemana, palavra);
            if (ocorrencias.Count > 0)
            {
                resultados.Add(new
                {
                    SemanaIndice = i + 1,
                    Ocorrencias = ocorrencias
                });
            }
        }

        return new { PalavraBuscada = palavra, ResultadosPorSemana = resultados };
    }
    [McpServerTool]
    [Description("Obtém a lição auxiliar da semana corrente.")]
    public async Task<LicaoAuxiliar?> Licao_GetCurrentWeekLessonAuxiliar()
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.CurrentSemana == null) return null;

        var licaoSemana = await _licaoService.GetLicaoAsync(licoes.CurrentSemana);
        return licaoSemana?.Auxiliar;
    }
    [McpServerTool]
    [Description("Obtém a lição auxiliar de uma semana específica do trimestre.")]
    public async Task<LicaoAuxiliar?> Licao_GetWeekLessonAuxiliar(
        [Description("Número da semana no trimestre (1-13).")][Required] int weekIndex)
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.Semanas == null || weekIndex < 1 || weekIndex > licoes.Semanas.Length)
            return null;

        var semanaSelecionada = licoes.Semanas[weekIndex - 1];
        var licaoSemana = await _licaoService.GetLicaoAsync(semanaSelecionada);
        return licaoSemana?.Auxiliar;
    }
    [McpServerTool]
    [Description("Obtém o informativo da semana corrente.")]
    public async Task<LicaoInformativo?> Licao_GetCurrentWeekLessonInformativo()
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.CurrentSemana == null) return null;

        var licaoSemana = await _licaoService.GetLicaoAsync(licoes.CurrentSemana);
        return licaoSemana?.Informativo;
    }
    [McpServerTool]
    [Description("Obtém o informativo de uma semana específica do trimestre.")]
    public async Task<LicaoInformativo?> Licao_GetWeekLessonInformativo(
        [Description("Número da semana no trimestre (1-13).")][Required] int weekIndex)
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.Semanas == null || weekIndex < 1 || weekIndex > licoes.Semanas.Length)
            return null;

        var semanaSelecionada = licoes.Semanas[weekIndex - 1];
        var licaoSemana = await _licaoService.GetLicaoAsync(semanaSelecionada);
        return licaoSemana?.Informativo;
    }
    [McpServerTool]
    [Description("Busca por múltiplas palavras-chave em todas as lições de uma semana específica.")]
    public async Task<object?> Licao_BuscarLicaoByWeekPalavrasChave(
        [Description("Número da semana no trimestre (1-13).")][Required] int weekIndex,
        [Description("Lista de palavras ou frases a serem buscadas.")][Required] IEnumerable<string> palavras)
    {
        var licoes = await _licaoService.GetLicoesAsync();
        if (licoes?.Semanas == null || weekIndex < 1 || weekIndex > licoes.Semanas.Length)
            return null;

        var semanaSelecionada = licoes.Semanas[weekIndex - 1];
        var licaoSemana = await _licaoService.GetLicaoAsync(semanaSelecionada);
        if (licaoSemana == null) return null;

        var resultados = new Dictionary<string, List<object>>();
        foreach (var palavra in palavras)
        {
            if (string.IsNullOrWhiteSpace(palavra)) continue;
            var ocorrencias = ExecutarBuscaPalavraChaveNaSemana(licaoSemana, palavra);
            if (ocorrencias.Count > 0)
            {
                resultados[palavra] = ocorrencias;
            }
        }

        return resultados;
    }
    [McpServerTool]
    [Description("Busca por múltiplas palavras-chave em todo o trimestre atual.")]
    public async Task<object> Licao_BuscarTrimestreCompletoPalavrasChave(
        [Description("Lista de palavras ou frases a serem buscadas em todo o trimestre.")][Required] IEnumerable<string> palavras)
    {
        var licoesCompletas = await _licaoService.GetAllLicoesTrimestreThrottledAsync(4);
        var resultados = new Dictionary<string, List<object>>();

        foreach (var palavra in palavras)
        {
            if (string.IsNullOrWhiteSpace(palavra)) continue;
            var resultadosPalavra = new List<object>();

            for (int i = 0; i < licoesCompletas.Count; i++)
            {
                var licaoSemana = licoesCompletas[i];
                var ocorrencias = ExecutarBuscaPalavraChaveNaSemana(licaoSemana, palavra);
                if (ocorrencias.Count > 0)
                {
                    resultadosPalavra.Add(new
                    {
                        SemanaIndice = i + 1,
                        Ocorrencias = ocorrencias
                    });
                }
            }

            if (resultadosPalavra.Count > 0)
            {
                resultados[palavra] = resultadosPalavra;
            }
        }

        return resultados;
    }

    #region Métodos Auxiliares de Tratamento e Busca

    private static object? ObterLicaoPorDiaSemana(LicaoSemanaData licao, DayOfWeek dia)
    {
        return dia switch
        {
            DayOfWeek.Sunday => licao.Domingo,
            DayOfWeek.Monday => licao.Segunda,
            DayOfWeek.Tuesday => licao.Terca,
            DayOfWeek.Wednesday => licao.Quarta,
            DayOfWeek.Thursday => licao.Quinta,
            DayOfWeek.Friday => licao.Sexta,
            DayOfWeek.Saturday => licao.Sabado,
            _ => null
        };
    }

    private static List<object> ExtrairTemasDaSemana(LicaoSemanaData licao)
    {
        var temas = new List<object>();

        if (licao.Sabado != null)
            temas.Add(new { Dia = DayOfWeek.Saturday, licao.Sabado.TitleLicao });
        if (licao.Domingo != null)
            temas.Add(new { Dia = DayOfWeek.Sunday, licao.Domingo.Title });
        if (licao.Segunda != null)
            temas.Add(new { Dia = DayOfWeek.Monday, licao.Segunda.Title });
        if (licao.Terca != null)
            temas.Add(new { Dia = DayOfWeek.Tuesday, licao.Terca.Title });
        if (licao.Quarta != null)
            temas.Add(new { Dia = DayOfWeek.Wednesday, licao.Quarta.Title });
        if (licao.Quinta != null)
            temas.Add(new { Dia = DayOfWeek.Thursday, licao.Quinta.Title });
        if (licao.Sexta != null)
            temas.Add(new { Dia = DayOfWeek.Friday, licao.Sexta.Title });
        if (licao.Auxiliar != null)
            temas.Add(new { Dia = "Auxiliar", licao.Auxiliar.Title });

        return temas;
    }

    private static List<object> ExecutarBuscaPalavraChaveNaSemana(LicaoSemanaData licao, string palavra)
    {
        var ocorrencias = new List<object>();
        var p = palavra.Trim();

        // Sábado
        if (licao.Sabado != null &&
            ((licao.Sabado.TitleLicao?.Contains(p, StringComparison.OrdinalIgnoreCase) ?? false) ||
             (licao.Sabado.Conteudo?.Contains(p, StringComparison.OrdinalIgnoreCase) ?? false) ||
             (licao.Sabado.VersoMemorizar?.Contains(p, StringComparison.OrdinalIgnoreCase) ?? false)))
        {
            ocorrencias.Add(new { Dia = DayOfWeek.Saturday, Titulo = licao.Sabado.TitleLicao });
        }

        // Dias úteis
        VerificarDiaEAdicionar(licao.Domingo, DayOfWeek.Sunday, p, ocorrencias);
        VerificarDiaEAdicionar(licao.Segunda, DayOfWeek.Monday, p, ocorrencias);
        VerificarDiaEAdicionar(licao.Terca, DayOfWeek.Tuesday, p, ocorrencias);
        VerificarDiaEAdicionar(licao.Quarta, DayOfWeek.Wednesday, p, ocorrencias);
        VerificarDiaEAdicionar(licao.Quinta, DayOfWeek.Thursday, p, ocorrencias);
        VerificarDiaEAdicionar(licao.Sexta, DayOfWeek.Friday, p, ocorrencias);

        // Auxiliar
        if (licao.Auxiliar != null &&
            ((licao.Auxiliar.Title?.Contains(p, StringComparison.OrdinalIgnoreCase) ?? false) ||
             (licao.Auxiliar.Conteudo?.Contains(p, StringComparison.OrdinalIgnoreCase) ?? false)))
        {
            ocorrencias.Add(new { Dia = "Auxiliar", Titulo = licao.Auxiliar.Title });
        }

        // Informativo
        if (licao.Informativo != null &&
            (licao.Informativo.Conteudo?.Contains(p, StringComparison.OrdinalIgnoreCase) ?? false))
        {
            ocorrencias.Add(new { Dia = "Informativo", Titulo = "Informativo Missionário" });
        }

        return ocorrencias;
    }

    private static void VerificarDiaEAdicionar(LicaoDia? dia, DayOfWeek diaSemana, string palavra, List<object> ocorrencias)
    {
        if (dia != null &&
            ((dia.Title?.Contains(palavra, StringComparison.OrdinalIgnoreCase) ?? false) ||
             (dia.Conteudo?.Contains(palavra, StringComparison.OrdinalIgnoreCase) ?? false) ||
             (dia.Rodape?.Contains(palavra, StringComparison.OrdinalIgnoreCase) ?? false)))
        {
            ocorrencias.Add(new { Dia = diaSemana, Titulo = dia.Title });
        }
    }

    #endregion
}