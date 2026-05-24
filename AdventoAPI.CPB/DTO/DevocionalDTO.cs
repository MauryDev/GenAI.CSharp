using System;
using System.Collections.Generic;
using System.Text;

namespace AdventoAPI.CPB.DTO;

public record DevocionalInfo(Uri Url, string DiadaSemanaNome, string DiaMesNome, string Title,
         string Content,
         string versoBiblico
);

public record DevocionalDiaInfo(DevocionalDayMonth Data, string Titulo, Uri Href);
public record DevocionalSemanaBloco(DevocionalDayMonth DataInicio, DevocionalDayMonth DataFinal, int NumberMeditacaoes, List<DevocionalDiaInfo> Dias);
public record MeditacaoInfo(string Title, string Description);

public record DevocionalDayMonth(int Day, int Month) : IComparable<DevocionalDayMonth>
{
    public int CompareTo(DevocionalDayMonth? other)
    {
        if (other is null) return 1;

        // Se estamos comparando meses muito distantes (ex: Jan contra Dez),
        // é um forte indício de virada de ano no contexto do devocional.
        int meuMesVirtual = this.Month;
        int outroMesVirtual = other.Month;

        
        if (meuMesVirtual != outroMesVirtual)
            return meuMesVirtual.CompareTo(outroMesVirtual);

        return this.Day.CompareTo(other.Day);
    }
    public static bool operator >(DevocionalDayMonth a, DevocionalDayMonth b) => a.CompareTo(b) > 0;
    public static bool operator <(DevocionalDayMonth a, DevocionalDayMonth b) => a.CompareTo(b) < 0;
    public static bool operator >=(DevocionalDayMonth a, DevocionalDayMonth b) => a.CompareTo(b) >= 0;
    public static bool operator <=(DevocionalDayMonth a, DevocionalDayMonth b) => a.CompareTo(b) <= 0;


}
public record MeditacoesHeader(DevocionalDayMonth DataInicio, DevocionalDayMonth DataFinal, int NumberMeditacaoes);