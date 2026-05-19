using System;
using System.Collections.Generic;
using System.Text;

namespace GenAI.CSharp.Models;

public record DevocionalInfo(string Url, string DiadaSemanaNome, string DiaMesNome, string Title,
         string Content,
         string versoBiblico
);