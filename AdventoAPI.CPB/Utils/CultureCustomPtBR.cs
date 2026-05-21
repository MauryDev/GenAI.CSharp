using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AdventoAPI.CPB.Utils
{
    internal static class CultureCustomPtBR
    {
        public static readonly CultureInfo PtBrCulture = CreateCustomPtBrCulture();

        private static CultureInfo CreateCustomPtBrCulture()
        {
            var culture = new CultureInfo("pt-BR");
            // Remove o ponto final de todas as abreviações (ex: "mai." vira "mai")
            culture.DateTimeFormat.AbbreviatedMonthNames = [.. culture.DateTimeFormat.AbbreviatedMonthNames.Select(m => m.TrimEnd('.'))];

            // Faz o mesmo para os nomes em minúsculo, por segurança
            culture.DateTimeFormat.AbbreviatedMonthGenitiveNames = [.. culture.DateTimeFormat.AbbreviatedMonthGenitiveNames.Select(m => m.TrimEnd('.'))];

            return culture;
        }
    }
}
