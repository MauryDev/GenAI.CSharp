namespace AdventoAPI.CPB.API;


public class DevocionalDiario(IHttpClientFactory httpClientFactory) : DevocionalBase(httpClientFactory)
{

    public override string BaseUrl => "https://mais.cpb.com.br/devocional-diario-ano/";

    public override string MeditacoesUrl => "https://mais.cpb.com.br/meditacoes-diarias/";
}
