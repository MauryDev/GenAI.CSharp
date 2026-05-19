using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;

namespace GenAI.CSharp.Models;

public record DevocionalSemanaBloco(DateOnly DataInicio, DateOnly DataFinal, int NumberMeditacaoes, List<DevocionalDiaInfo> Dias);

