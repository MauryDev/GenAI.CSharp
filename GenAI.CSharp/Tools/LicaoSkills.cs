using GenAI.CSharp.Models;
using GenAI.CSharp.Services;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GenAI.CSharp.Tools;

[McpServerToolType]
public class LicaoSkills
{
    LicaoService licaoService;
    public LicaoSkills(LicaoService licaoService)
    {
        this.licaoService = licaoService;
    }
    [McpServerTool]
    [Description("Obtém a lição do dia atual da semana corrente.")]
    public async Task<object> licao_getLicaoCorrenteDiaAtual()
    {   
        return await licaoService.GetDiaAtual();
    }
    [McpServerTool]
    [Description("0=domingo até 6=sábado")]
    public async Task<object> licao_getSemanaLicaoCorrenteByDiaSemana([Description("Obtém a lição por um dia da semana específico da semana corrente.")][Required] int index)
    {
        return await licaoService.GetLicaoByDia(index);
    }
    [McpServerTool]
    [Description("Obtém a lição de uma semana específica do trimestre por um dia específico.")]
    public async Task<object> licao_getLicaoByWeekAndDia([Description("Número da semana no trimestre (1-13)."), Required] int weekIndex, [Description("0=domingo até 6=sábado")][Required] int diaIndex)
    {
        return await licaoService.GetLicaoByWeekAndDia(weekIndex, diaIndex);
    }
    [McpServerTool]
    [Description("Obtém a lista de todos os temas/títulos das lições da semana corrente.")]
    public async Task<object> licao_getLicaoCorrenteTodosTemas()
    {
        return await licaoService.GetTodosTemas();
    }
    [McpServerTool]
    [Description("Obtém a lista de todos os temas/títulos das lições de uma semana específica.")]
    public async Task<object> licao_getLicaoByWeekTodosTemas([Description("Número da semana no trimestre (1-13).")][Required] int weekIndex)
    {
        return await licaoService.GetWeekTodosTemas(weekIndex);
    }
    [McpServerTool]
    [Description("Obtém o verso para memorizar da lição de sábado da semana corrente.")]
    public async Task<object> licao_getLicaoCorrenteVersoMemorizar()
    {
        return await licaoService.GetVersoMemorizar();
    }
    [McpServerTool]
    [Description("Obtém o verso para memorizar da lição de sábado de uma semana específica.")]
    public async Task<object> licao_getLicaoByWeekVersoMemorizar([Description("Número da semana no trimestre (1-13).")][Required] int weekIndex)
    {
        return await licaoService.GetWeekVersoMemorizar(weekIndex);
    }
    [McpServerTool]
    [Description("Busca por uma palavra-chave em todas as lições da semana corrente.")]
    public async Task<object> licao_buscarLicaoCorrentePalavraChave([Description("Palavra ou frase a ser buscada na semana corrente.")][Required] string palavra)
    {
        return await licaoService.BuscarPalavraChave(palavra);
    }
    [McpServerTool]
    [Description("Busca por uma palavra-chave em todas as lições de uma semana específica.")]
    public async Task<object> licao_buscarLicaoByWeekPalavraChave([Description("Número da semana no trimestre (1-13).")][Required] int weekIndex, [Description("Palavra ou frase a ser buscada.")][Required] string palavra)
    {
        return await licaoService.GetWeekBuscarPalavraChave(weekIndex, palavra);
    }
    [McpServerTool]
    [Description("Obtém a lição da semana corrente.")]
    public async Task<LicaoSemanaResumo> licao_getLicaoSemana()
    {
        return await licaoService.GetLicaoSemana();
    }
    [McpServerTool]
    [Description("Obtém a lição de um número específico do trimestre (começando em 1).")]
    public async Task<object> licao_getLicaoSemanaByIndex([Description("Número da lição no trimestre (ex: 1, 2, 3).")][Required] int index)
    {
        return await licaoService.GetLicaoByTrimestreIndex(index);
    }
    [McpServerTool]
    [Description("Obtém a lista de todas as lições do trimestre corrente.")]
    public async Task<LicaoSemanaResumo[]> licao_getLicoesTrimestre()
    {
        return await licaoService.GetLicoesTrimestre();
    }
    [McpServerTool]
    [Description("Busca por uma palavra-chave em todo o trimestre atual.")]
    public async Task<object> licao_buscarTrimestreCompletoPalavraChave([Description("Palavra ou frase a ser buscada em todo o trimestre.")][Required] string palavra)
    {
        return await licaoService.BuscarPalavraChaveTrimestre(palavra);
    }
    [McpServerTool]
    [Description("Obtém a lição auxiliar da semana corrente.")]
    public async Task<object> licao_getCurrentWeekLessonAuxiliar()
    {
        return await licaoService.GetCurrentWeekLessonAuxiliar();
    }
    [McpServerTool]
    [Description("Obtém a lição auxiliar de uma semana específica do trimestre.")]
    public async Task<object> licao_getWeekLessonAuxiliar([Description("Número da semana no trimestre (1-13).")][Required] int weekIndex)
    {
        return await licaoService.GetWeekLessonAuxiliar(weekIndex);
    }
    [McpServerTool]
    [Description("Obtém o informativo da semana corrente.")]
    public async Task<object> licao_getCurrentWeekLessonInformativo()
    {
        return await licaoService.GetCurrentWeekLessonInformativo();
    }
    [McpServerTool, Description("Obtém o informativo de uma semana específica do trimestre.")]
    public async Task<object> licao_getWeekLessonInformativo([Description("Número da semana no trimestre (1-13).")][Required] int weekIndex)
    {
        return await licaoService.GetWeekLessonInformativo(weekIndex);
    }
    
}
