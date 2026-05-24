namespace AdventoAPI.CPB.API;

public record LicaoAdultoOptions
{
    public record UrlsOptions
    {
        public string LicoesBaseUrl { get; init; } = "https://mais.cpb.com.br/licao-adultos/";
        public string AudiosBaseUrl { get; init; } = "https://mais.cpb.com.br/licao-adultos/audios/";
    }

    public record SelectorsOptions
    {
        // LicaoAdultoV2
        public string CardsContainer { get; init; } = ".cpbCards";
        public string LicoesList { get; init; } = ".licoes";
        public string AudioCards { get; init; } = ".cpbCards .mdl-card";
        public string AudioCardMedia { get; init; } = ".mdl-card__media";
        public string AudioCardTitle { get; init; } = ".mdl-card__title.mediaCardTitle h2";
        public string AudioCardVerso { get; init; } = "p.versoMemorizar";
        public string AudioCardLink { get; init; } = ".mdl-card__actions.mdl-card--border a";

        // LicaoParser
        public string TitleLicao { get; init; } = ".titleLicao";
        public string AudioList { get; init; } = "ul.audioList > li";
        public string AudioListContent { get; init; } = ".mdl-list__item-primary-content";
        public string SabadoBase { get; init; } = "#licaoSabado";
        public string ImageLicao { get; init; } = ".imageLicao";
        public string ConteudoLicao { get; init; } = ".conteudoLicaoDia";
        public string NumberLicao { get; init; } = ".numberLicao";
        public string DateLicao { get; init; } = ".descriptionText.dateLicao";
        public string DiaSabadoLicao { get; init; } = ".descriptionText.diaSabadoLicao";
        public string AnoBiblicoDia { get; init; } = ".descriptionText.anoBiblicoDia";
        public string VersoMemorizar { get; init; } = ".versoMemorizar";
        public string TitleLicaoDay { get; init; } = ".titleLicaoDay";
        public string RodapeLicaoDia { get; init; } = ".rodapeBoxLicaoDia.boxLicao";
        public string AuxiliarBase { get; init; } = "#licaoAuxiliar";
        public string AuxiliarNumber { get; init; } = ".descriptionText.numberLicao.numberLicaoAuxiliar";
        public string AuxiliarTitle { get; init; } = ".titleLicao.titleLicaoAuxiliar";
        public string InformativoBase { get; init; } = "#licaoInformativo";
        public string IsActiveDia { get; init; } = "is-active";
        public string LicaoDiaBase { get; init; } = "#licao";

        public string DescriptionText { get; init; } = ".descriptionText";
    }

    public UrlsOptions Urls { get; init; } = new();
    public SelectorsOptions Selectors { get; init; } = new();
    public string[] SectionNames { get; init; } = ["Sabado", "Domingo", "Segunda", "Terca", "Quarta", "Quinta", "Sexta", "Auxiliar", "Informativo", "Comentario"];

    public static LicaoAdultoOptions Default { get; } = new();
}