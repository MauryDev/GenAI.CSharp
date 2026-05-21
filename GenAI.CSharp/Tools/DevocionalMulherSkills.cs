using GenAI.CSharp.Models;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using AdventoAPI.CPB.API;

namespace GenAI.CSharp.Tools;

[McpServerToolType]
public class DevocionalMulherSkills : DevocionalSkillsBase
{
    public DevocionalMulherSkills(IHttpClientFactory httpClientFactory)
        : base(new DevocionalMulher(httpClientFactory.CreateClient()))
    {
    }

    [McpServerTool]
    [Description("devocional-mulher_today — returns today's devotional content for women.")]
    public async Task<DevocionalContentSafe> devocional_mulher_today()
        => await devocional_today();

    [McpServerTool]
    [Description("devocional-mulher_currentWeek — returns current week women devotionals content.")]
    public async Task<DevocionalSemanaSafe> devocional_mulher_currentWeek()
        => await devocional_currentWeek();

    [McpServerTool]
    [Description("devocional-mulher_weekDevocionais(int week) — returns the women devotionals for the given week index (1-based).")]
    public async Task<DevocionalSemanaSafe> devocional_mulher_weekDevocionais([Required][Description("1-based week index")] int week)
        => await devocional_weekDevocionais(week);

    [McpServerTool]
    [Description("devocional-mulher_getAllDevocionaisInfo — returns all women weeks and their days.")]
    public async Task<List<DevocionalSemanaSafe>> devocional_mulher_getAllDevocionaisInfo()
        => await devocional_getAllDevocionaisInfo();

    [McpServerTool]
    [Description("devocional-mulher_getDevocional(int day, int month) — returns women devotional content for the given day/month.")]
    public async Task<DevocionalContentSafe> devocional_mulher_getDevocional([Required][Description("day (1-31)")] int day, [Required][Description("month (1-12)")] int month)
        => await devocional_getDevocional(day, month);

    [McpServerTool]
    [Description("devocional_mulher_getMeditacaoInfo — returns info about women meditations.")]
    public async Task<List<MeditacaoSafe>> devocional_mulher_getMeditacaoInfo()
        => await devocional_getMeditacaoInfo();

    [McpServerTool]
    [Description("devocional-mulher_searchKeywordDevocionais(string keyword) — searches for a keyword across all women devotionals.")]
    public async Task<List<DevocionalContentSafe>> devocional_mulher_searchKeywordDevocionais([Required][Description("Keyword to search for")] string keyword)
        => await devocional_searchKeywordDevocionais(keyword);

    [McpServerTool]
    [Description("devocional-mulher_searchKeywordWeek(int weekIndex, string keyword) — searches for a keyword within a specific women week index.")]
    public async Task<List<DevocionalContentSafe>> devocional_mulher_searchKeywordWeek([Required][Description("0-based week index")] int weekIndex, [Required][Description("Keyword to search for")] string keyword)
        => await devocional_searchKeywordWeek(weekIndex, keyword);

    [McpServerTool]
    [Description("devocional-mulher_searchKeywordDateRange(int startDay, int startMonth, int endDay, int endMonth, string keyword) — searches for a keyword within a date range in women devotionals.")]
    public async Task<List<DevocionalContentSafe>> devocional_mulher_searchKeywordDateRange(
        [Required][Description("Start day (1-31)")] int startDay,
        [Required][Description("Start month (1-12)")] int startMonth,
        [Required][Description("End day (1-31)")] int endDay,
        [Required][Description("End month (1-12)")] int endMonth,
        [Required][Description("Keyword to search for")] string keyword)
        => await devocional_searchKeywordDateRange(startDay, startMonth, endDay, endMonth, keyword);

    [McpServerTool]
    [Description("devocional-mulher_searchKeywordsDevocionais — searches for multiple keywords across all women devotionals.")]
    public async Task<List<DevocionalContentSafe>> devocional_mulher_searchKeywordsDevocionais([Required] IEnumerable<string> keywords)
        => await devocional_searchKeywordsDevocionais(keywords);

    [McpServerTool]
    [Description("devocional-mulher_searchKeywordsWeek — searches for multiple keywords within a specific women week.")]
    public async Task<List<DevocionalContentSafe>> devocional_mulher_searchKeywordsWeek([Required] int weekIndex, [Required] IEnumerable<string> keywords)
        => await devocional_searchKeywordsWeek(weekIndex, keywords);

    [McpServerTool]
    [Description("devocional-mulher_searchKeywordsDateRange — searches for multiple keywords within a date range in women devotionals.")]
    public async Task<List<DevocionalContentSafe>> devocional_mulher_searchKeywordsDateRange(
        [Required] int startDay, [Required] int startMonth, [Required] int endDay, [Required] int endMonth, [Required] IEnumerable<string> keywords)
        => await devocional_searchKeywordsDateRange(startDay, startMonth, endDay, endMonth, keywords);
}