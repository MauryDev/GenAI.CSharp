using AdventoAPI.CPB.API;
using System.Net;

namespace AdventoAPI.CPB.Tests;

[TestClass]
public sealed class LicaoAdultoTests
{
    private readonly LicaoAdulto _service = new();

    [TestMethod]
    public async Task GetSabado_DeveRetornarObjetoValido()
    {
        // Arrange & Act
        var resultado = await _service.GetSabado(TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(resultado);
        Assert.AreEqual("Sábado", resultado.DiaSemana);
    }

    [TestMethod]
    public async Task GetDiaAtual_DeveRetornarDiaOuLancarExcecao()
    {
        // Act & Assert
        try
        {
            var resultado = await _service.GetDiaAtual();
            Assert.IsNotNull(resultado.DiaSemana);
        }
        catch (KeyNotFoundException)
        {
            Assert.Fail("Dia atual não encontrado fora do período letivo.");
        }
    }

    [TestMethod]
    public async Task GetLicoesTrimestre_DeveRetornarListaNaoVazia()
    {
        // Arrange & Act
        var licoes = await _service.GetLicoesTrimestre(TestContext.CancellationToken);

        // Assert
        Assert.IsNotEmpty(licoes);
    }

    [TestMethod]
    public async Task GetLicaoByDiaSemana_DeveRetornarDadosParaDomingo()
    {
        // Arrange (0 = Domingo)
        const int dia = 0;

        // Act
        var resultado = await _service.GetLicaoByDiaSemana(dia, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual("Domingo", resultado.DiaSemana);
        Assert.IsNotNull(resultado.Titulo);
    }

    [TestMethod]
    public async Task BuscarPalavraChaveTrimestre_DeveRetornarResultadosParaTermoExistente()
    {
        // Arrange
        const string termo = "Deus";

        // Act
        var resultado = await _service.BuscarPalavraChaveTrimestre(termo, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(resultado.Resultados);
    }
    [TestMethod]
    public void Testar_GetDiaNome()
    {
        var dias = Enumerable.Range(0,7);
        string[] diasNome = ["Domingo", "Segunda-Feira", "Terça-Feira", "Quarta-Feira", "Quinta-Feira", "Sexta-Feira", "Sábado"];
        var names = dias.Select(dia => LicaoAdulto.GetDiaSemana(dia));
        int i = 0;
        foreach (var nome in names) Assert.AreEqual(diasNome[i++], nome);
    }
    public TestContext TestContext { get; set; }
}