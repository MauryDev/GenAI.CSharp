using AdventoAPI.CPB.API;
using AdventoAPI.CPB.DTO;

namespace AdventoAPI.CPB.Tests
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var dataStr = "Dom 17/mai";
            var expectedValue = new DevocionalDayMonth(17,5);
            var valor = DevocionalBase.ParseDateCPBStyle(dataStr);

            Assert.AreEqual(expectedValue, valor);
        }
    }
}
