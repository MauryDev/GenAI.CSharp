using AdventoAPI.CPB.API;
using System.Net;

namespace AdventoAPI.CPB.Tests;

[TestClass]
public sealed class LicaoAdultoTests
{
    private readonly LicaoAdulto _service = new();

    [TestMethod]
    public async Task Testar_GetLicaoAsync()
    {

        var resultado = await _service.GetLicaoAsync("https://mais.cpb.com.br/licao/vivendo-pela-fe-2o-trimestre-2026/", TestContext.CancellationToken);
        if (resultado == null) return;


        TestContext.WriteLine(resultado.Sabado.Conteudo);
        Assert.DoesNotContain("Garanta", resultado.Sabado.Conteudo);
        Assert.IsNotNull(resultado.Segunda.Title);
        Assert.IsNotNull(resultado.Terca);
        Assert.IsNotNull(resultado.Sabado.AnoBiblicoDia);
    }

    [TestMethod]

    public async Task Testar_GetLicoesAsync()
    {
        var trimestreData = await _service.GetLicoesAsync(TestContext.CancellationToken);

        Assert.IsNotNull(trimestreData.Semanas[0]);
    }

    [TestMethod]
    public async Task Testar_Audios()
    {
        var audiosInfo = await _service.GetLicoesAudiosTrimestreAsync(TestContext.CancellationToken);

        Assert.IsNotNull(audiosInfo);

        var audiosSemana = await _service.GetLicaoSemanaAudiosAsync(audiosInfo[0].AudiosLink, TestContext.CancellationToken);

        Assert.IsNotEmpty(audiosSemana.Audios);
    }




    public TestContext TestContext { get; set; }
}