using GenAI.CSharp.Models;
using AdventoAPI.CPB.API;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace GenAI.CSharp.Tools;

[McpServerToolType]
public class DevocionalJovemSkills
{
    private readonly DevocionalJovem _devocional;

    public DevocionalJovemSkills(IHttpClientFactory httpClientFactory)
    {
        _devocional = new DevocionalJovem(httpClientFactory);
    }

    [McpServerTool]
    [Description("devocional-jovem_today — returns today's devotional content for youth.")]
    public async Task<DevocionalContentSafe> devocional_jovem_today()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return await devocional_jovem_getDevocional(today.Day, today.Month);
    }

    [McpServerTool]
    [Description("devocional-jovem_currentWeek — returns current week youth devotionals content.")]
    public async Task<DevocionalSemanaSafe> devocional_jovem_currentWeek()
    {
        return (await devocional_jovem_getAllDevocionaisInfo())[0];
    }

    [McpServerTool]
    [Description("devocional-jovem_weekDevocionais(int week) — returns the youth devotionals for the given week index (1-based).")]
    public async Task<DevocionalSemanaSafe> devocional_jovem_weekDevocionais([Required][Description("1-based week index")] int week)
    {
        if (week < 1) throw new ArgumentOutOfRangeException(nameof(week), "Week index must be 1 or greater.");

        var semanas = await _devocional.GetDevocionaisAsync();
        if (week > semanas.Count) throw new ArgumentOutOfRangeException(nameof(week), $"Week index out of range. There are {semanas.Count} weeks available.");

        var selected = semanas[semanas.Count - 1 - week];

        var diasSafe = selected.Dias.Select(d => new DevocionalDiaSafe(d.Data, d.Titulo)).ToList();

        return new DevocionalSemanaSafe(selected.DataInicio, selected.DataFinal, selected.NumberMeditacaoes, diasSafe);
    }

    [McpServerTool]
    [Description("devocional-jovem_getAllDevocionaisInfo — returns all youth weeks and their days.")]
    public async Task<List<DevocionalSemanaSafe>> devocional_jovem_getAllDevocionaisInfo()
    {
        var semanas = await _devocional.GetDevocionaisAsync();
        return [ ..semanas.Select(s => new DevocionalSemanaSafe(
                    s.DataInicio,
                    s.DataFinal,
                    s.NumberMeditacaoes,
                    s.Dias.Select(d => new DevocionalDiaSafe(d.Data, d.Titulo)).ToList()
                ))];
    }

    [McpServerTool]
    [Description("devocional-jovem_getDevocional(int day, int month) — returns youth devotional content for the given day/month.")]
    public async Task<DevocionalContentSafe> devocional_jovem_getDevocional([Required][Description("day (1-31)")] int day, [Required][Description("month (1-12)")] int month)
    {
        if (day < 1 || day > 31) throw new ArgumentOutOfRangeException(nameof(day));
        if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month));

        var semanas = await _devocional.GetDevocionaisAsync();

        var match = semanas.SelectMany(s => s.Dias).FirstOrDefault(d => d.Data.Day == day && d.Data.Month == month);

        if (match == null)
            throw new KeyNotFoundException($"No youth devotional found for {day}/{month}.");

        var href = match.Href;
        if (string.IsNullOrWhiteSpace(href))
            throw new InvalidOperationException("Youth devotional link is missing for the requested date.");

        var content = await _devocional.GetDevocional(href);

        return new DevocionalContentSafe(
            DiadaSemanaNome: content.DiadaSemanaNome ?? string.Empty,
            DiaMesNome: content.DiaMesNome ?? string.Empty,
            Title: content.Title ?? string.Empty,
            Content: content.Content ?? string.Empty,
            versoBiblico: content.versoBiblico ?? string.Empty
        );
    }

    [McpServerTool]
    [Description("devocional_jovem_getMeditacaoInfo — returns info about youth meditations.")]
    public async Task<List<MeditacaoSafe>> devocional_jovem_getMeditacaoInfo()
    {
        var meditacoes = await _devocional.GetMeditacaoInfoAsync();
        return [.. meditacoes.Select(m => new MeditacaoSafe(m.Title, m.Description))];
    }
}