namespace AdventoAPI.CPB.API;

public record LicaoAdultoOptions(
    string BaseUrl,
    string[] DiasSemanaIds,
    string SabadoSelector,
    string ConteudoLicaoDiaSelector,
    string TitleLicaoSelector,
    string VersoMemorizarSelector,
    string ActiveDaySelector,
    string TitleLicaoDaySelector,
    string LicaoCorrenteSelector,
    string LicoesSelector,
    string LicaoAuxiliarSelector,
    string LicaoAuxiliarNumberSelector,
    string LicaoAuxiliarTitleSelector,
    string LicaoInformativoSelector,
    int SemaphoreSlimInitial

)
{
    public static LicaoAdultoOptions Default { get; } = new(
        BaseUrl: "https://mais.cpb.com.br/licao-adultos/",
        DiasSemanaIds: ["licaoDomingo", "licaoSegunda", "licaoTerca", "licaoQuarta", "licaoQuinta", "licaoSexta", "licaoSabado"],
        SabadoSelector: "div#licaoSabado",
        ConteudoLicaoDiaSelector: ".conteudoLicaoDia",
        TitleLicaoSelector: ".titleLicao",
        VersoMemorizarSelector: ".versoMemorizar",
        ActiveDaySelector: "a.is-active",
        TitleLicaoDaySelector: ".titleLicaoDay",
        LicaoCorrenteSelector: "licao-corrente",
        LicoesSelector: ".licoes",
        LicaoAuxiliarSelector: "#licaoAuxiliar",
        LicaoAuxiliarNumberSelector: ".descriptionText .numberLicao .numberLicaoAuxiliar",
        LicaoAuxiliarTitleSelector: ".titleLicao .titleLicaoAuxiliar",
        LicaoInformativoSelector: "#licaoInformativo",
        SemaphoreSlimInitial: 5
    );
}