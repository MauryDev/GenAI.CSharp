namespace AdventoAPI.CPB.API;


public class DevocionalMulher(HttpClient? client = null) : DevocionalBase(client)
{

    public override string BaseUrl => "https://mais.cpb.com.br/devocional-mulher-ano/";

    public override string MeditacoesUrl => "https://mais.cpb.com.br/meditacao-da-mulher-2/";
}
