using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace GenAI.CSharp.Models
{
    internal record LicaoSemanaResumo
    (
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("verso")] string Verso,
        [property: JsonPropertyName("periodo")] string Periodo
    );
}
