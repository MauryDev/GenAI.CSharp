namespace AdventoAPI.CPB.API;

public class DevocionalJovem(HttpClient? client = null, DevocionalOptions? options = null) :
    DevocionalBase(client, options ?? DevocionalOptions.Jovem)
{
    
}
