using AdventoAPI.CPB.DTO;
using GenAI.CSharp.Models;
using System.Collections.Generic;  
namespace GenAI.CSharp.Models;

public record DevocionalSemanaSafe(DevocionalDayMonth DataInicio, DevocionalDayMonth DataFinal, int NumberMeditacaoes, List<DevocionalDiaSafe> Dias);
