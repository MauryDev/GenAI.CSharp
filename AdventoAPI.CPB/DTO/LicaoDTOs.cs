using System.Text.Json.Serialization;

namespace AdventoAPI.CPB.DTO;

public record LicaoAuxiliar
{
    public string? Number { get; init; }
    public string? Title { get; init; }
    public string? Conteudo { get; init; }
}

public record LicaoSabado
{
    public Uri? ImageUrl { get; set; }
    public string? NumberLicao { get; init; }
    public string? DateLicao { get; init; }
    public string? TitleLicao { get; init; }
    public string? DiaSabadoLicao { get; init; }
    public string? AnoBiblicoDia { get; init; }
    public string? VersoMemorizar { get; init; }
    public string? Conteudo { get; init; }
    public Uri? AudioUrl { get; init; }
    public Uri? YoutubeLink { get; init; }
    public bool IsActive { get; set; }
}

public record LicaoDia
{
    public LicaoDia_Data Data { get; init; }
    public string? AnoBiblicoDia { get; init; }
    public string? Title { get; init; }
    public string? Conteudo { get; init; }
    public Uri? AudioUrl { get; init; }
    public Uri? YoutubeLink { get; init; }
    public string? Rodape { get; init; }
    public bool IsActive { get; set; }

}
public class LicaoSemanaData
{
    public LicaoSabado? Sabado { get; init; }
    public LicaoDia? Domingo { get; init; }
    public LicaoDia? Segunda { get; init; }
    public LicaoDia? Terca { get; init; }
    public LicaoDia? Quarta { get; init; }
    public LicaoDia? Quinta { get; init; }
    public LicaoDia? Sexta { get; init; }
    public LicaoAuxiliar? Auxiliar { get; init; }
    public LicaoInformativo? Informativo { get; set; }
    public LicaoComentario? Comentario { get; init; }
}

public record LicaoInformativo
{
    public string? Conteudo { get; init; }
}

public record LicaoComentario { }


public record LicoesTrimestre
{
    public int Trimestre { get; init; }
    public int Ano { get; init; }
    public LicaoSemanaItem[]? Semanas { get; init; }
    public LicaoSemanaItem? CurrentSemana { get; init; }
}

public record LicaoSemanaItemDTO(
    [property: JsonPropertyName("img")] Uri Img,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("verso")] string Verso,
    [property: JsonPropertyName("periodo")] string Periodo,
    [property: JsonPropertyName("link")] Uri Link
);


public record LicaoSemanaItem(
    Uri Img,
    string Title,
    string Verso,
    LicaoSemanaPeriodoDatas Periodo,
    Uri Link
);

public record LicaoAudios(
    Uri CardMedia,
    string Title,
    string VersoMemorizar,
    Uri AudiosLink
);

public record LicaoSemanaAudiosResult(string Title, LicaoSemanaAudios[] Audios);

public record LicaoSemanaAudios(string Title, LicaoDia_Data Data, Uri AudioUrl);

public record LicaoDia_Data(DayOfWeek DiaDaSemana, int Dia, int Mes);


public record LicaoSemanaData_(int Day, int Month);
public record LicaoSemanaPeriodoDatas(LicaoSemanaData_ Start, LicaoSemanaData_ End);