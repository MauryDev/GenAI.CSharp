using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;

namespace AdventoAPI.CPB.DTO;

public record DevocionalSemanaBloco(DateOnly DataInicio, DateOnly DataFinal, int NumberMeditacaoes, List<DevocionalDiaInfo> Dias);

