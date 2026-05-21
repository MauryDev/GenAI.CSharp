using AdventoAPI.CPB.API;
using GenAI.CSharp.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GenAI.CSharp.Tools;

[McpServerToolType]
public class DevocionalJovemSkills : DevocionalSkillsBase
{
    public DevocionalJovemSkills(IHttpClientFactory httpClientFactory)
        : base(new DevocionalJovem(httpClientFactory.CreateClient()))
    {
    }

    [McpServerTool]
    [Description("devocional-jovem_today — returns today's devotional content for youth.")]
    public async Task<DevocionalContentSafe> devocional_jovem_today()
        => await devocional_today();

    [McpServerTool]
    [Description("devocional-jovem_currentWeek — returns current week youth devotionals content.")]
    public async Task<DevocionalSemanaSafe> devocional_jovem_currentWeek()
        => await devocional_currentWeek();

    [McpServerTool]
    [Description("devocional-jovem_weekDevocionais(int week) — returns the youth devotionals for the given week index (1-based).")]
    public async Task<DevocionalSemanaSafe> devocional_jovem_weekDevocionais([Required][Description("1-based week index")] int week)
        => await devocional_weekDevocionais(week);

    [McpServerTool]
    [Description("devocional-jovem_getAllDevocionaisInfo — returns all youth weeks and their days.")]
    public async Task<List<DevocionalSemanaSafe>> devocional_jovem_getAllDevocionaisInfo()
        => await devocional_getAllDevocionaisInfo();

    [McpServerTool]
    [Description("devocional-jovem_getDevocional(int day, int month) — returns youth devotional content for the given day/month.")]
    public async Task<DevocionalContentSafe> devocional_jovem_getDevocional([Required][Description("day (1-31)")] int day, [Required][Description("month (1-12)")] int month)
        => await devocional_getDevocional(day, month);

    [McpServerTool]
    [Description("devocional_jovem_getMeditacaoInfo — returns info about youth meditations.")]
    public async Task<List<MeditacaoSafe>> devocional_jovem_getMeditacaoInfo()
        => await devocional_getMeditacaoInfo();

    [McpServerTool]
    [Description("devocional-jovem_searchKeywordDevocionais(string keyword) — searches for a keyword across all youth devotionals.")]
    public async Task<List<DevocionalContentSafe>> devocional_jovem_searchKeywordDevocionais([Required][Description("Keyword to search for")] string keyword)
        => await devocional_searchKeywordDevocionais(keyword);

    [McpServerTool]
    [Description("devocional-jovem_searchKeywordWeek(int weekIndex, string keyword) — searches for a keyword within a specific youth week index.")]
    public async Task<List<DevocionalContentSafe>> devocional_jovem_searchKeywordWeek([Required][Description("0-based week index")] int weekIndex, [Required][Description("Keyword to search for")] string keyword)
        => await devocional_searchKeywordWeek(weekIndex, keyword);

    [McpServerTool]
    [Description("devocional-jovem_searchKeywordDateRange(int startDay, int startMonth, int endDay, int endMonth, string keyword) — searches for a keyword within a date range in youth devotionals.")]
    public async Task<List<DevocionalContentSafe>> devocional_jovem_searchKeywordDateRange(
        [Required][Description("Start day (1-31)")] int startDay,
        [Required][Description("Start month (1-12)")] int startMonth,
        [Required][Description("End day (1-31)")] int endDay,
        [Required][Description("End month (1-12)")] int endMonth,
        [Required][Description("Keyword to search for")] string keyword)
        => await devocional_searchKeywordDateRange(startDay, startMonth, endDay, endMonth, keyword);

    [McpServerTool]
    [Description("devocional-jovem_searchKeywordsDevocionais — searches for multiple keywords across all youth devotionals.")]
    public async Task<List<DevocionalContentSafe>> devocional_jovem_searchKeywordsDevocionais([Required] IEnumerable<string> keywords)
        => await devocional_searchKeywordsDevocionais(keywords);

    [McpServerTool]
    [Description("devocional-jovem_searchKeywordsWeek — searches for multiple keywords within a specific youth week.")]
    public async Task<List<DevocionalContentSafe>> devocional_jovem_searchKeywordsWeek([Required] int weekIndex, [Required] IEnumerable<string> keywords)
        => await devocional_searchKeywordsWeek(weekIndex, keywords);

    [McpServerTool]
    [Description("devocional-jovem_searchKeywordsDateRange — searches for multiple keywords within a date range in youth devotionals.")]
    public async Task<List<DevocionalContentSafe>> devocional_jovem_searchKeywordsDateRange(
        [Required] int startDay, [Required] int startMonth, [Required] int endDay, [Required] int endMonth, [Required] IEnumerable<string> keywords)
        => await devocional_searchKeywordsDateRange(startDay, startMonth, endDay, endMonth, keywords);
}