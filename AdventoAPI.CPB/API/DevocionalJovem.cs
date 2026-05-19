namespace AdventoAPI.CPB.API;

public class DevocionalJovem(HttpClient? client = null) : DevocionalBase(client)
{
    public override string BaseUrl => "https://mais.cpb.com.br/devocional-jovem-ano/";

    public override string MeditacoesUrl => "https://mais.cpb.com.br/meditacao-jovem/";
}
