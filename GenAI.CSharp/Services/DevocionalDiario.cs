using AngleSharp;
using AngleSharp.Dom;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GenAI.CSharp.Services;


public class DevocionalDiario(IHttpClientFactory httpClientFactory) : DevocionalBase(httpClientFactory)
{

    public override string BaseUrl => "https://mais.cpb.com.br/devocional-diario-ano/";

    public override string MeditacoesUrl => "https://mais.cpb.com.br/meditacoes-diarias/";
}
