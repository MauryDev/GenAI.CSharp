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
public class DevocionalMulherSkills
{
    private readonly DevocionalMulher _devocional;

    public DevocionalMulherSkills(DevocionalMulher devocional)
    {
        _devocional = devocional;
    }

    [McpServerTool]
    [Description("devocional-mulher_today — returns today's devotional content for women.")]
    public async Task<DevocionalContentSafe> devocional_mulher_today()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return await devocional_mulher_getDevocional(today.Day, today.Month);
    }

    [McpServerTool]
    [Description("devocional-mulher_currentWeek — returns current week women devotionals content.")]
    public async Task<DevocionalSemanaSafe> devocional_mulher_currentWeek()
    {
        return (await devocional_mulher_getAllDevocionaisInfo())[0];
    }

    [McpServerTool]
    [Description("devocional-mulher_weekDevocionais(int week) — returns the women devotionals for the given week index (1-based).")]
    public async Task<DevocionalSemanaSafe> devocional_mulher_weekDevocionais([Required][Description("1-based week index")] int week)
    {
        if (week < 1) throw new ArgumentOutOfRangeException(nameof(week), "Week index must be 1 or greater.");

        var semanas = await _devocional.GetDevocionaisAsync();
        if (week > semanas.Count) throw new ArgumentOutOfRangeException(nameof(week), $"Week index out of range. There are {semanas.Count} weeks available.");

        var selected = semanas[semanas.Count - 1 - week];

        var diasSafe = selected.Dias.Select(d => new DevocionalDiaSafe(d.Data, d.Titulo)).ToList();

        return new DevocionalSemanaSafe(selected.DataInicio, selected.DataFinal, selected.NumberMeditacaoes, diasSafe);
    }

    [McpServerTool]
    [Description("devocional-mulher_getAllDevocionaisInfo — returns all women weeks and their days.")]
    public async Task<List<DevocionalSemanaSafe>> devocional_mulher_getAllDevocionaisInfo()
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
    [Description("devocional-mulher_getDevocional(int day, int month) — returns women devotional content for the given day/month.")]
    public async Task<DevocionalContentSafe> devocional_mulher_getDevocional([Required][Description("day (1-31)")] int day, [Required][Description("month (1-12)")] int month)
    {
        if (day < 1 || day > 31) throw new ArgumentOutOfRangeException(nameof(day));
        if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month));

        var semanas = await _devocional.GetDevocionaisAsync();

        var match = semanas.SelectMany(s => s.Dias).FirstOrDefault(d => d.Data.Day == day && d.Data.Month == month);

        if (match == null)
            throw new KeyNotFoundException($"No women devotional found for {day}/{month}.");

        var href = match.Href;
        if (string.IsNullOrWhiteSpace(href))
            throw new InvalidOperationException("Women devotional link is missing for the requested date.");

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
    [Description("devocional_mulher_getMeditacaoInfo — returns info about women meditations.")]
    public async Task<List<MeditacaoSafe>> devocional_mulher_getMeditacaoInfo()
    {
        var meditacoes = await _devocional.GetMeditacaoInfoAsync();
        return [.. meditacoes.Select(m => new MeditacaoSafe(m.Title, m.Description))];
    }
}