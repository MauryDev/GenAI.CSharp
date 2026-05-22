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
        var resultado = await _service.GetSabado();

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
            Assert.IsTrue(true, "Dia atual não encontrado fora do período letivo.");
        }
    }

    [TestMethod]
    public async Task GetLicoesTrimestre_DeveRetornarListaNaoVazia()
    {
        // Arrange & Act
        var licoes = await _service.GetLicoesTrimestre();

        // Assert
        Assert.IsNotEmpty(licoes);
    }

    [TestMethod]
    public async Task GetLicaoByDiaSemana_DeveRetornarDadosParaDomingo()
    {
        // Arrange (0 = Domingo)
        int dia = 0;

        // Act
        var resultado = await _service.GetLicaoByDiaSemana(dia);

        // Assert
        Assert.AreEqual("Domingo", resultado.DiaSemana);
        Assert.IsNotNull(resultado.Titulo);
    }

    [TestMethod]
    public async Task BuscarPalavraChaveTrimestre_DeveRetornarResultadosParaTermoExistente()
    {
        // Arrange
        var termo = "Deus";

        // Act
        var resultado = await _service.BuscarPalavraChaveTrimestre(termo);

        // Assert
        Assert.IsNotNull(resultado.Resultados);
    }
}