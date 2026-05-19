using System;
using System.Collections.Generic;
using System.Text;

namespace GenAI.CSharp.Services
{
    public class DevocionalJovem(IHttpClientFactory httpClientFactory) : DevocionalBase(httpClientFactory)
    {
        public override string BaseUrl => "https://mais.cpb.com.br/devocional-jovem-ano/";

        public override string MeditacoesUrl => "https://mais.cpb.com.br/meditacao-jovem/";
    }
}
