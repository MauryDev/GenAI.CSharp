using System;
using System.Collections.Generic;
using System.Text;

namespace AdventoAPI.CPB.DTO;

public record DevocionalInfo(string Url, string DiadaSemanaNome, string DiaMesNome, string Title,
         string Content,
         string versoBiblico
);