using AdventoAPI.CPB.API;
using GenAI.CSharp.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GenAI.CSharp.Tools;

[McpServerToolType]
public class DevocionalDiarioSkills : DevocionalSkillsBase
{
    public DevocionalDiarioSkills(IHttpClientFactory httpClientFactory)
        : base(new DevocionalDiario(httpClientFactory.CreateClient()))
    {
    }

    [McpServerTool]
    [Description("devocional-diario_today — returns today's devotional content.")]
    public async Task<DevocionalContentSafe> devocional_diario_today()
        => await devocional_today();

    [McpServerTool]
    [Description("devocional-diario_currentWeek — returns current week devotionals content.")]
    public async Task<DevocionalSemanaSafe> devocional_diario_currentWeek()
        => await devocional_currentWeek();

    [McpServerTool]
    [Description("devocional-diario_weekDevocionais(int week) — returns the devotionals for the given week index (1-based).")]
    public async Task<DevocionalSemanaSafe> devocional_diario_weekDevocionais([Required][Description("1-based week index")] int week)
        => await devocional_weekDevocionais(week);

    [McpServerTool]
    [Description("devocional-diario_getAllDevocionaisInfo — returns all weeks and their days.")]
    public async Task<List<DevocionalSemanaSafe>> devocional_diario_getAllDevocionaisInfo()
        => await devocional_getAllDevocionaisInfo();

    [McpServerTool]
    [Description("devocional-diario_getDevocional(int day, int month) — returns devotional content for the given day/month.")]
    public async Task<DevocionalContentSafe> devocional_diario_getDevocional([Required][Description("day (1-31)")] int day, [Required][Description("month (1-12)")] int month)
        => await devocional_getDevocional(day, month);

    [McpServerTool]
    [Description("devocional_diario_getMeditacaoInfo — returns info about meditation.")]
    public async Task<List<MeditacaoSafe>> devocional_diario_getMeditacaoInfo()
        => await devocional_getMeditacaoInfo();

    [McpServerTool]
    [Description("devocional-diario_searchKeywordDevocionais(string keyword) — searches for a keyword across all devotionals.")]
    public async Task<List<DevocionalContentSafe>> devocional_diario_searchKeywordDevocionais([Required][Description("Keyword to search for")] string keyword)
        => await devocional_searchKeywordDevocionais(keyword);

    [McpServerTool]
    [Description("devocional-diario_searchKeywordWeek(int weekIndex, string keyword) — searches for a keyword within a specific week index.")]
    public async Task<List<DevocionalContentSafe>> devocional_diario_searchKeywordWeek([Required][Description("0-based week index")] int weekIndex, [Required][Description("Keyword to search for")] string keyword)
        => await devocional_searchKeywordWeek(weekIndex, keyword);

    [McpServerTool]
    [Description("devocional-diario_searchKeywordDateRange(int startDay, int startMonth, int endDay, int endMonth, string keyword) — searches for a keyword within a date range.")]
    public async Task<List<DevocionalContentSafe>> devocional_diario_searchKeywordDateRange(
        [Required][Description("Start day (1-31)")] int startDay,
        [Required][Description("Start month (1-12)")] int startMonth,
        [Required][Description("End day (1-31)")] int endDay,
        [Required][Description("End month (1-12)")] int endMonth,
        [Required][Description("Keyword to search for")] string keyword)
        => await devocional_searchKeywordDateRange(startDay, startMonth, endDay, endMonth, keyword);

    [McpServerTool]
    [Description("devocional-diario_searchKeywordsDevocionais — searches for multiple keywords across all devotionals.")]
    public async Task<List<DevocionalContentSafe>> devocional_diario_searchKeywordsDevocionais([Required] IEnumerable<string> keywords)
        => await devocional_searchKeywordsDevocionais(keywords);

    [McpServerTool]
    [Description("devocional-diario_searchKeywordsWeek — searches for multiple keywords within a specific week.")]
    public async Task<List<DevocionalContentSafe>> devocional_diario_searchKeywordsWeek([Required] int weekIndex, [Required] IEnumerable<string> keywords)
        => await devocional_searchKeywordsWeek(weekIndex, keywords);

    [McpServerTool]
    [Description("devocional-diario_searchKeywordsDateRange — searches for multiple keywords within a date range.")]
    public async Task<List<DevocionalContentSafe>> devocional_diario_searchKeywordsDateRange(
        [Required] int startDay, [Required] int startMonth, [Required] int endDay, [Required] int endMonth, [Required] IEnumerable<string> keywords)
        => await devocional_searchKeywordsDateRange(startDay, startMonth, endDay, endMonth, keywords);
}