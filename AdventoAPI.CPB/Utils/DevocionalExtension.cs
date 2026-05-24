using AdventoAPI.CPB.API;
using AdventoAPI.CPB.DTO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AdventoAPI.CPB.Utils;

public static partial class DevocionalExtension
{
    extension (DevocionalDiaInfo devocionalSemanaBloco)
    {
        public Task<DevocionalInfo> GetDevocionalDia(DevocionalBase devocionalBase,CancellationToken cancellationToken = default)
        {
            return devocionalBase.GetDevocionalDia(devocionalSemanaBloco.Href, cancellationToken);
        }

    }
    extension (DevocionalBase devocional)
    {
        public async Task<List<DevocionalInfo>> BuscarPalavraChaveDevocionais(string palavra, CancellationToken cancellationToken = default)
        {
            return await devocional.BuscarPalavrasChaveDevocionais(palavra.ToSingleIEnumerable(), cancellationToken);
        }

        public async Task<List<DevocionalInfo>> BuscarPalavrasChaveDevocionais(IEnumerable<string> palavras, CancellationToken cancellationToken = default)
        {
            var blocos = await devocional.GetDevocionaisAsync(cancellationToken);
            return await devocional.ProcessarDevocionaisAsync(blocos.SelectMany(b => b.Dias), palavras, cancellationToken);
        }

        public async Task<List<DevocionalInfo>> BuscarPalavraChaveSemana(int semanaIndex, string palavra, CancellationToken cancellationToken = default)
        {
            return await devocional.BuscarPalavrasChaveSemana(semanaIndex, palavra.ToSingleIEnumerable(), cancellationToken);
        }

        public async Task<List<DevocionalInfo>> BuscarPalavrasChaveSemana(int semanaIndex, IEnumerable<string> palavras, CancellationToken cancellationToken = default)
        {
            var blocos = await devocional.GetDevocionaisAsync(cancellationToken);
            if (semanaIndex < 0 || semanaIndex >= blocos.Count) return [];

            var dias = blocos.OrderBy(e => e.DataFinal).ElementAt(semanaIndex).Dias;
            return await devocional.ProcessarDevocionaisAsync(dias, palavras, cancellationToken);

        }

        internal async Task<List<DevocionalInfo>> ProcessarDevocionaisAsync(IEnumerable<DevocionalDiaInfo> dias, IEnumerable<string> palavras, CancellationToken cancellationToken = default)
        {
            using var semaphore = new SemaphoreSlim(devocional.Options.SemaphoreSlimInitial);

            var tarefas = dias.Select(async dia =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await devocional.GetDevocionalDia(dia.Href, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            return await Task.WhenEach(tarefas)
                .Select((e, i, token) => e.AsValueTask())
                .Where(info => info != null && palavras.Any(p => info.Content?.Contains(p, StringComparison.OrdinalIgnoreCase) ?? false))
                .OfType<DevocionalInfo>()
                .ToListAsync(cancellationToken);
        }


        public async Task<List<DevocionalInfo>> BuscarPalavraChaveDataRange(DevocionalDayMonth dataInicio, DevocionalDayMonth dataFim, string palavra, CancellationToken cancellationToken = default)
        {
            return await devocional.BuscarPalavrasChaveDataRange(dataInicio, dataFim, palavra.ToSingleIEnumerable(), cancellationToken);
        }

        public async Task<List<DevocionalInfo>> BuscarPalavrasChaveDataRange(DevocionalDayMonth dataInicio, DevocionalDayMonth dataFim, IEnumerable<string> palavras, CancellationToken cancellationToken = default)
        {
            var blocos = await devocional.GetDevocionaisAsync(cancellationToken);
            var dias = blocos.Where(b => b.DataFinal <= dataFim && b.DataInicio >= dataInicio)
                             .SelectMany(b => b.Dias)
                             .Where(d => d.Data >= dataInicio && d.Data <= dataFim);

            return await devocional.ProcessarDevocionaisAsync(dias, palavras, cancellationToken);
        }

    }

    extension (string _this)
    {
        internal MeditacoesHeader ParseHeader()
        {
            var match = ParserHeaderRegex().Match(_this);

            
            if (!match.Success)
                throw new FormatException($"Invalid header format: {_this}");

            return new(
                DataInicio: match.Groups[1].Value.ParseDateCPBStyle(),
                DataFinal: match.Groups[2].Value.ParseDateCPBStyle(),
                NumberMeditacaoes: int.Parse(match.Groups[3].Value)
            );
        }

        internal DevocionalDayMonth ParseDateCPBStyle()
        {
            ArgumentException.ThrowIfNullOrEmpty(_this, nameof(_this));



            ReadOnlySpan<char> dateSpan = _this.AsSpan();

            var enumerator = ParserDataRegex().EnumerateMatches(dateSpan);

            if (enumerator.MoveNext())
            {
                dateSpan = dateSpan[enumerator.Current.Length..];
            }

            if (!DateTime.TryParseExact(dateSpan, "d/MMM", CultureCustomPtBR.PtBrCulture, DateTimeStyles.None, out var parsedDate))
            {
                throw new FormatException($"Invalid date format: {_this}");
            }

            return new DevocionalDayMonth(parsedDate.Day, parsedDate.Month);
        }

    }

    [GeneratedRegex(@"(\d{1,2}/\w{3})\s*–\s*(\d{1,2}/\w{3})\s*\((\d+)\s*meditações\)")]
    private static partial Regex ParserHeaderRegex();
    [GeneratedRegex(@"^\w{3}\s+")]
    private static partial Regex ParserDataRegex();
}


