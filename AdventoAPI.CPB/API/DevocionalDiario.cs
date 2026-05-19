namespace AdventoAPI.CPB.API;


public class DevocionalDiario(HttpClient? client = null) : DevocionalBase(client)
{

    public override string BaseUrl => "https://mais.cpb.com.br/devocional-diario-ano/";

    public override string MeditacoesUrl => "https://mais.cpb.com.br/meditacoes-diarias/";
}
