using GenAI.CSharp.Models;
using GenAI.CSharp.Services;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GenAI.CSharp.Tools;

[McpServerToolType]
public class DevocionalDiarioSkills
{
    private readonly DevocionalDiario _devocionalDiario;

    public DevocionalDiarioSkills(DevocionalDiario devocionalDiario)
    {
        _devocionalDiario = devocionalDiario;
    }


    [McpServerTool]
    [Description("devocional-diario_today — returns today's devotional content.")]
    public async Task<DevocionalContentSafe> devocional_diario_today()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return await devocional_diario_getDevocional(today.Day, today.Month);
    }

    [McpServerTool]
    [Description("devocional-diario_today — returns current week devotionais content.")]
    public async Task<DevocionalSemanaSafe> devocional_diario_currentWeek()
    {
        return (await devocional_diario_getAllDevocionaisInfo())[0];
    }

    [McpServerTool]
    [Description("devocional-diario_weekDevocionais(int week) — returns the devocionais for the given week index (1-based).")]
    public async Task<DevocionalSemanaSafe> devocional_diario_weekDevocionais([Required][Description("1-based week index")] int week)
    {
        if (week < 1) throw new ArgumentOutOfRangeException(nameof(week), "Week index must be 1 or greater.");

        var semanas = await _devocionalDiario.GetDevocionaisAsync();
        if (week > semanas.Count) throw new ArgumentOutOfRangeException(nameof(week), $"Week index out of range. There are {semanas.Count} weeks available.");

        var selected = semanas[semanas.Count - 1 - week];

        var diasSafe = selected.Dias.Select(d => new DevocionalDiaSafe(d.Data, d.Titulo)).ToList();

        return new DevocionalSemanaSafe(selected.DataInicio, selected.DataFinal, selected.NumberMeditacaoes, diasSafe);
    }

    [McpServerTool]
    [Description("devocional-diario_getAllDevocionaisInfo — returns all weeks and their days.")]
    public async Task<List<DevocionalSemanaSafe>> devocional_diario_getAllDevocionaisInfo()
    {
        var semanas = await _devocionalDiario.GetDevocionaisAsync();
        return [ ..semanas.Select(s => new DevocionalSemanaSafe(
                    s.DataInicio,
                    s.DataFinal,
                    s.NumberMeditacaoes,
                    s.Dias.Select(d => new DevocionalDiaSafe(d.Data, d.Titulo)).ToList()
                ))];
    }

    [McpServerTool]
    [Description("devocional-diario_getDevocional(int day, int month) — returns devotional content for the given day/month.")]
    public async Task<DevocionalContentSafe> devocional_diario_getDevocional([Required][Description("day (1-31)")] int day, [Required][Description("month (1-12)")] int month)
    {
        if (day < 1 || day > 31) throw new ArgumentOutOfRangeException(nameof(day));
        if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month));

        var semanas = await _devocionalDiario.GetDevocionaisAsync();

        // Find the DiaInfo matching provided day and month. Use internal href only to fetch content.
        var match = semanas.SelectMany(s => s.Dias).FirstOrDefault(d => d.Data.Day == day && d.Data.Month == month);

        if (match == null)
            throw new KeyNotFoundException($"No devotional found for {day}/{month}.");

        // The href is considered unsafe to expose; use it internally to fetch the content.
        var href = match.Href;
        if (string.IsNullOrWhiteSpace(href))
            throw new InvalidOperationException("Devotional link is missing for the requested date.");

        var content = await _devocionalDiario.GetDevocional(href);

        return new DevocionalContentSafe(
            DiadaSemanaNome: content.DiadaSemanaNome ?? string.Empty,
            DiaMesNome: content.DiaMesNome ?? string.Empty,
            Title: content.Title ?? string.Empty,
            Content: content.Content ?? string.Empty,
            versoBiblico: content.versoBiblico ?? string.Empty
        );
    }

    [McpServerTool]
    [Description("devocional_diario_getMeditacaoInfo — returns a info about meditacion.")]
    public async Task<List<MeditacaoSafe>> devocional_diario_getMeditacaoInfo()
    {
        var meditacoes = await _devocionalDiario.GetMeditacaoInfoAsync();
        return [.. meditacoes.Select(m => new MeditacaoSafe(m.Title, m.Description))];
    }
}
