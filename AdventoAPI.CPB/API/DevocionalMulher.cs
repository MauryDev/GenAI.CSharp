namespace AdventoAPI.CPB.API;


public class DevocionalMulher(HttpClient? client = null, DevocionalOptions? options = null) :
    DevocionalBase(client, options ?? DevocionalOptions.Mulher)
{
}
