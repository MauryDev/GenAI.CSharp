using System.Text.Json.Serialization;

namespace AdventoAPI.CPB.DTO;

public record LicaoBuscaTrimestreResultado(int Semana, string Dia, string Titulo, string Conteudo);
public record LicaoBuscaTrimestreResponse(List<LicaoBuscaTrimestreResultado> Resultados);
public record LicaoAuxiliarDTO(string NumeroLicao, string Titulo, string Conteudo);
public record LicaoInformativoDTO(string Conteudo);
public record LicaoVersoMemorizarDTO(string Verso);

public record LicaoTemaDTO(string Dia, string Tema);
public record LicaoTemasResponse(IEnumerable<LicaoTemaDTO> Temas);

public record LicaoAdultoDay(string DiaSemana, string Titulo, string Conteudo, string VersoParaMemorizar);


public record LicaoAdultoSemana(
 [property: JsonPropertyName("img")] string Img,
 [property: JsonPropertyName("title")] string Title,
 [property: JsonPropertyName("verso")] string Verso,
 [property: JsonPropertyName("periodo")] string Periodo,
 [property: JsonPropertyName("link")] string Link
);

public record LicaoAdultoTema(string Dia, string Tema);

public record LicaoBuscaResponse(List<LicaoBuscaResultado> Resultados);
public record LicaoBuscaResultado(string Dia, string Titulo, string Conteudo);
