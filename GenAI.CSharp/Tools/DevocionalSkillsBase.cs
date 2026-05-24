using AdventoAPI.CPB.API;
using AdventoAPI.CPB.DTO;
using AdventoAPI.CPB.Utils;
using GenAI.CSharp.Models;
using System.ComponentModel;


namespace GenAI.CSharp.Tools;

public abstract class DevocionalSkillsBase(DevocionalBase devocionalBase)
{
    readonly DevocionalBase _devocional = devocionalBase;


    public async Task<DevocionalContentSafe> devocional_today()
    {

        var today = DateOnly.FromDateTime(DateTime.Now);
        return await devocional_getDevocional(today.Day, today.Month);
    }

    public async Task<DevocionalSemanaSafe> devocional_currentWeek()
    {
        return (await devocional_getAllDevocionaisInfo())[0];
    }


    public async Task<DevocionalSemanaSafe> devocional_weekDevocionais(int week)
    {
        if (week < 1) throw new ArgumentOutOfRangeException(nameof(week), "Week index must be 1 or greater.");

        var semanas = await _devocional.GetDevocionaisAsync();
        if (week > semanas.Count) throw new ArgumentOutOfRangeException(nameof(week), $"Week index out of range. There are {semanas.Count} weeks available.");

        var selected = semanas[semanas.Count - 1 - week];

        var diasSafe = selected.Dias.Select(d => new DevocionalDiaSafe(d.Data, d.Titulo)).ToList();

        return new DevocionalSemanaSafe(selected.DataInicio, selected.DataFinal, selected.NumberMeditacaoes, diasSafe);
    }

    public async Task<List<DevocionalSemanaSafe>> devocional_getAllDevocionaisInfo()
    {
        var semanas = await _devocional.GetDevocionaisAsync();
        return [ ..semanas.Select(s => new DevocionalSemanaSafe(
                            s.DataInicio,
                            s.DataFinal,
                            s.NumberMeditacaoes,
                            s.Dias.Select(d => new DevocionalDiaSafe(d.Data, d.Titulo)).ToList()
                        ))];
    }

    public async Task<DevocionalContentSafe> devocional_getDevocional(int day, int month)
    {
        if (day < 1 || day > 31) throw new ArgumentOutOfRangeException(nameof(day));
        if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month));

        var semanas = await _devocional.GetDevocionaisAsync();

        var match = semanas.SelectMany(s => s.Dias).FirstOrDefault(d => d.Data.Day == day && d.Data.Month == month) ?? throw new KeyNotFoundException($"No devotional found for {day}/{month}.");        

        var content = await match.GetDevocionalDia(_devocional);

        return new DevocionalContentSafe(
            DiadaSemanaNome: content.DiadaSemanaNome ?? string.Empty,
            DiaMesNome: content.DiaMesNome ?? string.Empty,
            Title: content.Title ?? string.Empty,
            Content: content.Content ?? string.Empty,
            versoBiblico: content.versoBiblico ?? string.Empty
        );
    }

    public async Task<List<MeditacaoSafe>> devocional_getMeditacaoInfo()
    {
        var meditacoes = await _devocional.GetMeditacaoInfoAsync();
        return [.. meditacoes.Select(m => new MeditacaoSafe(m.Title, m.Description))];
    }

    public async Task<List<DevocionalContentSafe>> devocional_searchKeywordDevocionais(string keyword)
    {
        var results = await _devocional.BuscarPalavraChaveDevocionais(keyword);
        return [.. results.Select(c => new DevocionalContentSafe(
            c.DiadaSemanaNome ?? string.Empty,
            c.DiaMesNome ?? string.Empty,
            c.Title ?? string.Empty,
            c.Content ?? string.Empty,
            c.versoBiblico ?? string.Empty
        ))];
    }

    public async Task<List<DevocionalContentSafe>> devocional_searchKeywordWeek(int weekIndex, string keyword)
    {
        var results = await _devocional.BuscarPalavraChaveSemana(weekIndex, keyword);
        return [.. results.Select(c => new DevocionalContentSafe(
            c.DiadaSemanaNome ?? string.Empty,
            c.DiaMesNome ?? string.Empty,
            c.Title ?? string.Empty,
            c.Content ?? string.Empty,
            c.versoBiblico ?? string.Empty
        ))];
    }

    public async Task<List<DevocionalContentSafe>> devocional_searchKeywordDateRange(
        int startDay,
        int startMonth,
        int endDay,
        int endMonth,
        string keyword)
    {
        var start = new DevocionalDayMonth(startDay, startMonth);
        var end = new DevocionalDayMonth(endDay, endMonth);

        var results = await _devocional.BuscarPalavraChaveDataRange(start, end, keyword);
        return [.. results.Select(c => new DevocionalContentSafe(
            c.DiadaSemanaNome ?? string.Empty,
            c.DiaMesNome ?? string.Empty,
            c.Title ?? string.Empty,
            c.Content ?? string.Empty,
            c.versoBiblico ?? string.Empty
        ))];
    }

    public async Task<List<DevocionalContentSafe>> devocional_searchKeywordsDevocionais(IEnumerable<string> keywords)
    {
        var results = await _devocional.BuscarPalavrasChaveDevocionais(keywords);
        return [.. results.Select(c => new DevocionalContentSafe(
            c.DiadaSemanaNome ?? string.Empty,
            c.DiaMesNome ?? string.Empty,
            c.Title ?? string.Empty,
            c.Content ?? string.Empty,
            c.versoBiblico ?? string.Empty
        ))];
    }

    public async Task<List<DevocionalContentSafe>> devocional_searchKeywordsWeek(int weekIndex, IEnumerable<string> keywords)
    {
        var results = await _devocional.BuscarPalavrasChaveSemana(weekIndex, keywords);
        return [.. results.Select(c => new DevocionalContentSafe(
            c.DiadaSemanaNome ?? string.Empty,
            c.DiaMesNome ?? string.Empty,
            c.Title ?? string.Empty,
            c.Content ?? string.Empty,
            c.versoBiblico ?? string.Empty
        ))];
    }

    public async Task<List<DevocionalContentSafe>> devocional_searchKeywordsDateRange(
        int startDay,
        int startMonth,
        int endDay,
        int endMonth,
        IEnumerable<string> keywords)
    {
        var start = new DevocionalDayMonth(startDay, startMonth);
        var end = new DevocionalDayMonth(endDay, endMonth);

        var results = await _devocional.BuscarPalavrasChaveDataRange(start, end, keywords);
        return [.. results.Select(c => new DevocionalContentSafe(
            c.DiadaSemanaNome ?? string.Empty,
            c.DiaMesNome ?? string.Empty,
            c.Title ?? string.Empty,
            c.Content ?? string.Empty,
            c.versoBiblico ?? string.Empty
        ))];
    }

}
