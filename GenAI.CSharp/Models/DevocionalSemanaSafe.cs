using GenAI.CSharp.Models;
using System.Collections.Generic;  
namespace GenAI.CSharp.Models;

public record DevocionalSemanaSafe(DateOnly DataInicio, DateOnly DataFinal, int NumberMeditacaoes, List<DevocionalDiaSafe> Dias);
