using AdventoAPI.CPB.DTO;
using AngleSharp.Dom;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AdventoAPI.CPB.API;

internal static partial class LicaoParser
{
    [GeneratedRegex(@"url\((.*?)\)")]
    private static partial Regex GetUrlRegex();

    extension (IDocument document)
    {
        public LicaoSemanaData ParseSemana()
        {
            return new LicaoSemanaData
            {
                Sabado = document.ParseSabado(),
                Domingo = document.GetDiaInfo(1),
                Segunda = document.GetDiaInfo(2),
                Terca = document.GetDiaInfo(3),
                Quarta = document.GetDiaInfo(4),
                Quinta = document.GetDiaInfo(5),
                Sexta = document.GetDiaInfo(6),
                Auxiliar = document.GetAuxiliaryLesson(),
                Informativo = document.GetInformativeLesson()
            };
        }

        public LicaoSemanaAudiosResult ParseSemanaAudios()
        {
            var options = LicaoAdultoOptions.Default;
            var title = document.QuerySelector(options.Selectors.TitleLicao)?.TextContent?.Trim();
            var audioElements = document.QuerySelectorAll(options.Selectors.AudioList);

            var audios = audioElements.Select(li =>
            {
                var contentSpan = li.QuerySelector(options.Selectors.AudioListContent);
                var spans = contentSpan?.QuerySelectorAll("span").ToList();

                return new LicaoSemanaAudios
                (
                    Title: spans?.ElementAtOrDefault(0)?.TextContent?.Trim(),
                    Data: spans?.ElementAtOrDefault(1)?.TextContent?.Trim().Obter_DiaSemana_Dia_Mes(),
                    AudioUrl: li.QuerySelector("audio")?.GetAttribute("src").TryParseUri()
                );
            }).ToArray();

            return new LicaoSemanaAudiosResult
            (
                Title: title,
                Audios: audios
            );
        }

        LicaoSabado? ParseSabado()
        {
            var options = LicaoAdultoOptions.Default;
            var baseElement = document.QuerySelector(options.Selectors.SabadoBase);
            if (baseElement == null) return null;

            var imageStyle = baseElement.QuerySelector(options.Selectors.ImageLicao)?.GetAttribute("style");
            var imageUrl = GetUrlRegex().Match(imageStyle ?? "").Groups[1].Value;
            var conteudoLicaoElement = baseElement.QuerySelector(options.Selectors.ConteudoLicao);

            conteudoLicaoElement?.ChildNodes
                .OfType<IElement>()
                .Where(e => e.TagName == "P" && e.ClassList.Length == 0)
                .ToList()
                .ForEach(e => e.Remove());

            var audioElem = conteudoLicaoElement?.QuerySelector("audio");
            var audioLink = audioElem?.GetAttribute("src") ?? string.Empty;
            audioElem?.Remove();

            return new LicaoSabado
            {
                IsActive = baseElement.ClassList.Contains(options.Selectors.IsActiveDia),
                ImageUrl = imageUrl.TryParseUri(),
                NumberLicao = baseElement.QuerySelector(options.Selectors.NumberLicao)?.TextContent?.Trim(),
                DateLicao = baseElement.QuerySelector(options.Selectors.DateLicao)?.TextContent?.Trim(),
                TitleLicao = baseElement.QuerySelector(options.Selectors.TitleLicao)?.TextContent?.Trim(),
                DiaSabadoLicao = baseElement.QuerySelector(options.Selectors.DiaSabadoLicao)?.TextContent?.Trim(),
                AnoBiblicoDia = baseElement.QuerySelector(options.Selectors.AnoBiblicoDia)?.TextContent?.Trim(),
                VersoMemorizar = baseElement.QuerySelector(options.Selectors.VersoMemorizar)?.TextContent?.Trim(),
                Conteudo = conteudoLicaoElement?.Text().Trim(),
                AudioUrl = audioLink.TryParseUri(),
                YoutubeLink = conteudoLicaoElement?.QuerySelector("iframe")?.GetAttribute("src").TryParseUri()
            };
        }

        LicaoDia? GetDiaInfo(int dayIndex)
        {
            var options = LicaoAdultoOptions.Default;
            var baseElement = document.QuerySelector($"{options.Selectors.LicaoDiaBase}{options.SectionNames[dayIndex]}");
            if (baseElement == null) return null;

            var conteudoLicaoElement = baseElement.QuerySelector(options.Selectors.ConteudoLicao);

            conteudoLicaoElement?.ChildNodes
                .OfType<IElement>()
                .Where(e => e.TagName == "P" && e.ClassList.Length == 0)
                .ToList()
                .ForEach(e => e.Remove());

            var audioElement = conteudoLicaoElement.QuerySelector("audio");
            var audiolink = audioElement.GetAttribute("src");
            audioElement.Remove();

            return new LicaoDia
            {
                IsActive = baseElement.ClassList.Contains(options.Selectors.IsActiveDia),
                Data = baseElement
                    ?.QuerySelector(options.Selectors.DescriptionText)
                    ?.TextContent
                    ?.Trim()
                    ?.Obter_DiaSemana_Dia_Mes()
                ,
                AnoBiblicoDia = baseElement.QuerySelector(options.Selectors.AnoBiblicoDia)?.TextContent?.Trim(),
                Title = baseElement.QuerySelector(options.Selectors.TitleLicaoDay)?.TextContent?.Trim(),
                Conteudo = conteudoLicaoElement?.Text().Trim(),
                AudioUrl = audiolink.TryParseUri(),
                YoutubeLink = conteudoLicaoElement.QuerySelector("iframe")?.GetAttribute("src").TryParseUri(),
                Rodape = baseElement.QuerySelector(options.Selectors.RodapeLicaoDia)?.TextContent?.Trim()
            };
        }

        private LicaoAuxiliar GetAuxiliaryLesson()
        {
            var options = LicaoAdultoOptions.Default;
            var baseElement = document.QuerySelector(options.Selectors.AuxiliarBase);
            return new LicaoAuxiliar
            {
                Number = baseElement?.QuerySelector(options.Selectors.AuxiliarNumber)?.TextContent?.Trim(),
                Title = baseElement?.QuerySelector(options.Selectors.AuxiliarTitle)?.TextContent?.Trim(),
                Conteudo = baseElement?.QuerySelector(options.Selectors.ConteudoLicao)?.Text().Trim()
            };
        }

        LicaoInformativo GetInformativeLesson()
        {
            var options = LicaoAdultoOptions.Default;
            var baseElement = document.QuerySelector(options.Selectors.InformativoBase);
            return new LicaoInformativo
            {
                Conteudo = baseElement?.QuerySelector(options.Selectors.ConteudoLicao)?.Text().Trim()
            };
        }

    }
    
    extension (string _this)
    {
        internal Uri? TryParseUri() =>
        Uri.TryCreate(_this, UriKind.Absolute, out var uri) ? uri : null;

        internal Uri? ExtractUrlFromStyle()
        {
            if (string.IsNullOrEmpty(_this)) return null;
            var match = GetUrlRegex().Match(_this);
            return match.Groups[1].Value.TryParseUri();
        }
        internal LicaoDia_Data Obter_DiaSemana_Dia_Mes()
        {
            var match = Regex_Obter_DiaSemana_Dia_Mes().Match(_this);

            if (!match.Success)
            {
                throw new FormatException("A string não está no formato esperado 'DiaDaSemana, Dia de Mês'");
            }


            var diaSemana = match.Groups[1].Value.Trim().ObterDiaDaSemanaPorNome();
            int dia = int.Parse(match.Groups[2].Value);
            var mes = match.Groups[3].Value.Trim().ObterNumeroDoMesPorNome();


            return new(diaSemana.Value, dia, mes.Value);
        }

        public DayOfWeek? ObterDiaDaSemanaPorNome()
        {
            if (string.IsNullOrWhiteSpace(_this)) return null;

            string busca = _this.Trim();

            var cultura = CultureInfo.GetCultureInfo("pt-BR");

            return Enum.GetValues<DayOfWeek>()
                .Select(dayofWeek => new
                {
                    EnumValue = dayofWeek,
                    Name = cultura.DateTimeFormat.GetDayName(dayofWeek).ToLower()

                }
                ).FirstOrDefault(e => e.Name.StartsWith(busca, StringComparison.OrdinalIgnoreCase))
                ?.EnumValue;

        }

        public int? ObterNumeroDoMesPorNome()
        {
            if (string.IsNullOrWhiteSpace(_this)) return null;

            string busca = _this.Trim();

            var cultura = CultureInfo.GetCultureInfo("pt-BR");
            string[] mesesDoAno = cultura.DateTimeFormat.MonthNames;

            return mesesDoAno
                .Take(12)
                .Select((e, i) => new { Name = e, Index = i + 1 })
                .FirstOrDefault(mesInfo => mesInfo.Name.StartsWith(busca, StringComparison.OrdinalIgnoreCase))
                ?.Index;

        }

        public int ExtrairNumeroTrimestre()
        {
            if (string.IsNullOrWhiteSpace(_this))
                throw new ArgumentException("A string de entrada não pode ser vazia.");

            Match match = Regex_ExtrairNumeroTrimestre().Match(_this);

            if (match.Success)
            {
                return int.Parse(match.Value);
            }

            throw new FormatException("Nenhum número de trimestre foi encontrado na string.");
        }

        public LicaoSemanaPeriodoDatas ObterLicaoSemanaPeriodo()
        {

            ArgumentException.ThrowIfNullOrWhiteSpace(_this, nameof(_this));

            string texto = _this.Trim().ToLower();

            var matchDoisMeses = Regex_ObterLicaoSemanaPeriodo().Match(texto);
            if (matchDoisMeses.Success)
            {
                int diaInicio = int.Parse(matchDoisMeses.Groups[1].Value);
                int? mesInicio = matchDoisMeses.Groups[2].Value.ObterNumeroDoMesPorNome();
                int diaFim = int.Parse(matchDoisMeses.Groups[3].Value);
                int? mesFim = matchDoisMeses.Groups[4].Value.ObterNumeroDoMesPorNome();

                if (mesInicio.HasValue && mesFim.HasValue)
                {
                    return new LicaoSemanaPeriodoDatas(
                        new LicaoSemanaData_(diaInicio, mesInicio.Value),
                        new LicaoSemanaData_(diaFim, mesFim.Value)
                    );
                }
            }

            var matchMesmoMes = Regex_ObterLicaoSemanaPeriodo2().Match(texto);
            if (matchMesmoMes.Success)
            {
                int diaInicio = int.Parse(matchMesmoMes.Groups[1].Value);
                int diaFim = int.Parse(matchMesmoMes.Groups[2].Value);
                int? mes = matchMesmoMes.Groups[3].Value.ObterNumeroDoMesPorNome();

                if (mes.HasValue)
                {
                    return new LicaoSemanaPeriodoDatas(
                        new LicaoSemanaData_(diaInicio, mes.Value),
                        new LicaoSemanaData_(diaFim, mes.Value)
                    );
                }
            }

            throw new FormatException("O formato do intervalo de datas não é reconhecido.");
        }

       
    }

    extension (LicaoSemanaItem licaoSemanaItem)
    {
        public async Task<LicaoSemanaAudiosResult?> GetLicaoSemanaAudiosAsync(LicaoAdulto licaoAdulto, CancellationToken cancellationToken = default)
        {
            var document = await licaoAdulto.GetDocumentAsync(licaoSemanaItem.Link, cancellationToken);
            return document.ParseSemanaAudios();
        }
    }

    

    [GeneratedRegex(@"^\s*([^,]+)\s*,\s*(\d+)\s+de\s+([^\s]+)\s*$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex Regex_Obter_DiaSemana_Dia_Mes();
    [GeneratedRegex(@"\d+")]
    private static partial Regex Regex_ExtrairNumeroTrimestre();
    [GeneratedRegex(@"^(\d+)\s+de\s+([^\s]+)\s+a\s+(\d+)\s+de\s+([^\s]+)$")]
    private static partial Regex Regex_ObterLicaoSemanaPeriodo();
    [GeneratedRegex(@"^(\d+)\s+a\s+(\d+)\s+de\s+([^\s]+)$")]
    private static partial Regex Regex_ObterLicaoSemanaPeriodo2();
}