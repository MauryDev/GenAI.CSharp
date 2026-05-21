using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace GenAI.CSharp.Models;

public record LicaoAdultoSemanaSafe(
    string Title,
    string Verso,
    string Periodo
);
