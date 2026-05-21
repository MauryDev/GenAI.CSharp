namespace AdventoAPI.CPB.API;




public record DevocionalOptions(
    string BaseUrl,
    string MeditacoesUrl,
    string SemanaBlocoSelector,
    string HeaderSelector,
    string DiasListaSelector,
    string CardSelector,
    string TitleSelector,
    string DescriptionSelector,
    string MeditacaoTitleSelector,
    string DiaSemanaSelector,
    string DiaMesSelector,
    string VersoBiblicoSelector,
    string ContentSelector
)
{
    private static DevocionalOptions CreateWithDefaults(string baseUrl, string meditacoesUrl) => new(
        BaseUrl: baseUrl,
        MeditacoesUrl: meditacoesUrl,
        SemanaBlocoSelector: ".semana-bloco",
        HeaderSelector: ".semana-header span",
        DiasListaSelector: ".semana-body .dias-lista .dia-item a",
        CardSelector: ".cpbCards",
        TitleSelector: ".mediaCardTitle",
        DescriptionSelector: ".mdl-card__supporting-text",
        MeditacaoTitleSelector: ".titleMeditacao",
        DiaSemanaSelector: ".descriptionText.diaSemanaMeditacao",
        DiaMesSelector: ".descriptionText.diaMesMeditacao",
        VersoBiblicoSelector: ".descriptionText.versoBiblico",
        ContentSelector: ".conteudoMeditacao"
    );

    public static DevocionalOptions Diario => CreateWithDefaults(
        "https://mais.cpb.com.br/devocional-diario-ano/",
        "https://mais.cpb.com.br/meditacoes-diarias/"
    );

    public static DevocionalOptions Jovem => CreateWithDefaults(
        "https://mais.cpb.com.br/devocional-jovem-ano/",
        "https://mais.cpb.com.br/meditacao-jovem/"
    );

    public static DevocionalOptions Mulher => CreateWithDefaults(
        "https://mais.cpb.com.br/devocional-mulher-ano/",
        "https://mais.cpb.com.br/meditacao-da-mulher-2/"
    );

    public static DevocionalOptions Default => CreateWithDefaults("", "");
}