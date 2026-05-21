namespace AdventoAPI.CPB.API;


public class DevocionalDiario(HttpClient? client = null, DevocionalOptions? options = null) :
    DevocionalBase(client, options ?? DevocionalOptions.Diario)
{
}
