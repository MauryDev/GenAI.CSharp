using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AdventoAPI.CPB.DTO;

public record LicaoAdultoSemanaResumo
(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("verso")] string Verso,
    [property: JsonPropertyName("periodo")] string Periodo
);
