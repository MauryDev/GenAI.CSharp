namespace AdventoAPI.CPB.DTO;

public record LicaoBuscaTrimestreResultado(int Semana, string Dia, string Titulo, string Conteudo);
public record LicaoBuscaTrimestreResponse(List<LicaoBuscaTrimestreResultado> Resultados);
public record LicaoAuxiliarDTO(string NumeroLicao, string Titulo, string Conteudo);
public record LicaoInformativoDTO(string Conteudo);
public record LicaoVersoMemorizarDTO(string Verso);

public record LicaoTemaDTO(string Dia, string Tema);
public record LicaoTemasResponse(IEnumerable<LicaoTemaDTO> Temas);