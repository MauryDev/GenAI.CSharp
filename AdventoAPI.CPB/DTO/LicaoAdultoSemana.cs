using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AdventoAPI.CPB.DTO;

public record LicaoAdultoSemana(
 [property: JsonPropertyName("img")] string Img,
 [property: JsonPropertyName("title")] string Title,
 [property: JsonPropertyName("verso")] string Verso,
 [property: JsonPropertyName("periodo")] string Periodo,
 [property: JsonPropertyName("link")] string Link
);