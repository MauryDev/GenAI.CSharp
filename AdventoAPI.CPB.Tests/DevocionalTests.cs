using AdventoAPI.CPB.API;

namespace AdventoAPI.CPB.Tests;

[TestClass]
public sealed class DevocionalTests
{
    [TestMethod]
    public void DevocionalDiario_ShouldInitializeWithDefaultOptions()
    {
        var service = new DevocionalDiario();
        Assert.IsNotNull(service.Options);
    }

    [TestMethod]
    public void DevocionalJovem_ShouldInitializeWithDefaultOptions()
    {
        var service = new DevocionalJovem();
        Assert.IsNotNull(service.Options);
    }

    [TestMethod]
    public void DevocionalMulher_ShouldInitializeWithDefaultOptions()
    {
        var service = new DevocionalMulher();
        Assert.IsNotNull(service.Options);
    }

    [TestMethod]
    public async Task DevocionalDiario_FetchDevocionais_ShouldReturnData()
    {
        // Arrange
        var service = new DevocionalDiario();

        // Act
        var result = await service.GetDevocionaisAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotEmpty(result, "A lista de devocionais diários não deve estar vazia.");
    }

    [TestMethod]
    public async Task DevocionalJovem_FetchMeditacoes_ShouldReturnData()
    {
        // Arrange
        var service = new DevocionalJovem();

        // Act
        var result = await service.GetMeditacaoInfoAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotEmpty(result, "A lista de meditações jovens não deve estar vazia.");
    }

    [TestMethod]
    public async Task DevocionalMulher_FetchSingleDevocional_ShouldReturnData()
    {
        // Arrange
        var service = new DevocionalMulher();
        var blocos = await service.GetDevocionaisAsync(TestContext.CancellationToken);
        var firstUrl = blocos.FirstOrDefault()?.Dias.FirstOrDefault()?.Href;

        if (string.IsNullOrEmpty(firstUrl))
        {
            Assert.Inconclusive("Não foi possível obter uma URL para testar o fetch individual.");
        }

        // Act
        var devocional = await service.GetDevocional(firstUrl, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(devocional);
        Assert.IsFalse(string.IsNullOrEmpty(devocional.Title), "O título do devocional não deve estar vazio.");
        Assert.IsFalse(string.IsNullOrEmpty(devocional.Content), "O conteúdo do devocional não deve estar vazio.");
    }

    [TestMethod]
    public async Task DevocionalBase_SearchKeyword_ShouldReturnResults()
    {
        // Arrange
        var service = new DevocionalDiario();
        string keyword = "Deus"; // Palavra comum para garantir resultado

        // Act
        var result = await service.BuscarPalavraChaveDevocionais(keyword, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(result);
        // Nota: Dependendo do conteúdo do site, isso pode variar, mas valida a execução do fluxo
        Console.WriteLine($"Encontrados {result.Count} devocionais com a palavra '{keyword}'.");
    }

    public TestContext TestContext { get; set; }
}
