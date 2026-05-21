using AdventoAPI.CPB.API;
using AdventoAPI.CPB.DTO;
using GenAI.CSharp.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace GenAI.CSharp.Tools;

[McpServerToolType]
public class LicaoAdultoSkills
{
    LicaoAdulto licaoService;
    public LicaoAdultoSkills(IHttpClientFactory httpClientFactory)
    {
        this.licaoService = new LicaoAdulto(httpClientFactory.CreateClient());
    }
    [McpServerTool]
    [Description("Obtém a lição do dia atual da semana corrente.")]
    public async Task<LicaoAdultoDay> licao_getLicaoCorrenteDiaAtual()
    {   
        return await licaoService.GetDiaAtual();
    }
    [McpServerTool]
    [Description("0=domingo até 6=sábado")]
    public async Task<LicaoAdultoDay> licao_getSemanaLicaoCorrenteByDiaSemana([Description("Obtém a lição por um dia da semana específico da semana corrente.")][Required] int index)
    {
        return await licaoService.GetLicaoByDiaSemana(index);
    }
    [McpServerTool]
    [Description("Obtém a lição de uma semana específica do trimestre por um dia específico.")]
    public async Task<LicaoAdultoDay> licao_getLicaoByWeekAndDia([Description("Número da semana no trimestre (1-13)."), Required] int weekIndex, [Description("0=domingo até 6=sábado")][Required] int diaIndex)
    {
        return await licaoService.GetLicaoByWeekAndDiaSemana(weekIndex, diaIndex);
    }
    [McpServerTool]
    [Description("Obtém a lista de todos os temas/títulos das lições da semana corrente.")]
    public async Task<LicaoTemasResponse> licao_getLicaoCorrenteTodosTemas()
    {
        return await licaoService.GetTodosTemas();
    }
    [McpServerTool]
    [Description("Obtém a lista de todos os temas/títulos das lições de uma semana específica.")]
    public async Task<LicaoTemasResponse> licao_getLicaoByWeekTodosTemas([Description("Número da semana no trimestre (1-13).")][Required] int weekIndex)
    {
        return await licaoService.GetWeekTodosTemas(weekIndex);
    }
    [McpServerTool]
    [Description("Obtém o verso para memorizar da lição de sábado da semana corrente.")]
    public async Task<LicaoVersoMemorizarDTO> licao_getLicaoCorrenteVersoMemorizar()
    {
        return await licaoService.GetVersoMemorizar();
    }
    [McpServerTool]
    [Description("Obtém o verso para memorizar da lição de sábado de uma semana específica.")]
    public async Task<LicaoVersoMemorizarDTO> licao_getLicaoByWeekVersoMemorizar([Description("Número da semana no trimestre (1-13).")][Required] int weekIndex)
    {
        return await licaoService.GetWeekVersoMemorizar(weekIndex);
    }
    [McpServerTool]
    [Description("Busca por uma palavra-chave em todas as lições da semana corrente.")]
    public async Task<LicaoBuscaResponse> licao_buscarLicaoCorrentePalavraChave([Description("Palavra ou frase a ser buscada na semana corrente.")][Required] string palavra)
    {
        return await licaoService.BuscarPalavraChave(palavra);
    }
    [McpServerTool]
    [Description("Busca por uma palavra-chave em todas as lições de uma semana específica.")]
    public async Task<LicaoBuscaResponse> licao_buscarLicaoByWeekPalavraChave([Description("Número da semana no trimestre (1-13).")][Required] int weekIndex, [Description("Palavra ou frase a ser buscada.")][Required] string palavra)
    {
        return await licaoService.GetWeekBuscarPalavraChave(weekIndex, palavra);
    }
    [McpServerTool]
    [Description("Obtém a lição da semana corrente.")]
    public async Task<LicaoAdultoSemanaSafe> licao_getLicaoSemana()
    {
        var licaoAdultoSemana = await licaoService.GetLicaoSemana();
        return new LicaoAdultoSemanaSafe(
            Title: licaoAdultoSemana.Title,
            Periodo: licaoAdultoSemana.Periodo,
            Verso: licaoAdultoSemana.Verso
        );
    }
    [McpServerTool]
    [Description("Obtém a lição de um número específico do trimestre (começando em 1).")]
    public async Task<LicaoAdultoSemanaSafe> licao_getLicaoSemanaByIndex([Description("Número da lição no trimestre (ex: 1, 2, 3).")][Required] int index)
    {
        var licaoAdultoSemana = await licaoService.GetLicaoSemanaByIndex(index); 
        return new LicaoAdultoSemanaSafe(
            Title: licaoAdultoSemana.Title,
            Periodo: licaoAdultoSemana.Periodo,
            Verso: licaoAdultoSemana.Verso
        );
    }
    [McpServerTool]
    [Description("Obtém a lista de todas as lições do trimestre corrente.")]
    public async Task<LicaoAdultoSemanaSafe[]> licao_getLicoesTrimestre()
    {
        var retVal = await licaoService.GetLicoesTrimestre();

        return [.. retVal.Select((licaoAdultoSemana) => new LicaoAdultoSemanaSafe(
                Title: licaoAdultoSemana.Title,
                Periodo: licaoAdultoSemana.Periodo,
                Verso: licaoAdultoSemana.Verso
            ))
        ];
    }
    [McpServerTool]
    [Description("Busca por uma palavra-chave em todo o trimestre atual.")]
    public async Task<LicaoBuscaTrimestreResponse> licao_buscarTrimestreCompletoPalavraChave([Description("Palavra ou frase a ser buscada em todo o trimestre.")][Required] string palavra)
    {
        return await licaoService.BuscarPalavraChaveTrimestre(palavra);
    }
    [McpServerTool]
    [Description("Obtém a lição auxiliar da semana corrente.")]
    public async Task<LicaoAuxiliarDTO> licao_getCurrentWeekLessonAuxiliar()
    {
        return await licaoService.GetCurrentWeekLessonAuxiliar();
    }
    [McpServerTool]
    [Description("Obtém a lição auxiliar de uma semana específica do trimestre.")]
    public async Task<LicaoAuxiliarDTO> licao_getWeekLessonAuxiliar([Description("Número da semana no trimestre (1-13).")][Required] int weekIndex)
    {
        return await licaoService.GetWeekLessonAuxiliar(weekIndex);
    }
    [McpServerTool]
    [Description("Obtém o informativo da semana corrente.")]
    public async Task<LicaoInformativoDTO> licao_getCurrentWeekLessonInformativo()
    {
        return await licaoService.GetCurrentWeekLessonInformativo();
    }
    [McpServerTool, Description("Obtém o informativo de uma semana específica do trimestre.")]
    public async Task<LicaoInformativoDTO> licao_getWeekLessonInformativo([Description("Número da semana no trimestre (1-13).")][Required] int weekIndex)
    {
        return await licaoService.GetWeekLessonInformativo(weekIndex);
    }

    [McpServerTool]
    [Description("Busca por múltiplas palavras-chave em todas as lições de uma semana específica.")]
    public async Task<LicaoBuscaResponse> licao_buscarLicaoByWeekPalavrasChave([Description("Número da semana no trimestre (1-13).")][Required] int weekIndex, [Description("Lista de palavras ou frases a serem buscadas.")][Required] IEnumerable<string> palavras)
    {
        return await licaoService.GetWeekBuscarPalavrasChave(weekIndex, palavras);
    }
    [McpServerTool]
    [Description("Busca por múltiplas palavras-chave em todo o trimestre atual.")]
    public async Task<LicaoBuscaTrimestreResponse> licao_buscarTrimestreCompletoPalavrasChave([Description("Lista de palavras ou frases a serem buscadas em todo o trimestre.")][Required] IEnumerable<string> palavras)
    {
        return await licaoService.BuscarPalavrasChaveTrimestre(palavras);
    }

}
