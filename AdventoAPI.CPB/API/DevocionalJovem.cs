namespace AdventoAPI.CPB.API;

public class DevocionalJovem(IHttpClientFactory httpClientFactory) : DevocionalBase(httpClientFactory)
{
    public override string BaseUrl => "https://mais.cpb.com.br/devocional-jovem-ano/";

    public override string MeditacoesUrl => "https://mais.cpb.com.br/meditacao-jovem/";
}
