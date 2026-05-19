using GenAI.CSharp.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenAI.CSharp.Services
{
    public static class MainService
    {
       
        public static IServiceCollection AddGenAiLesson(this IServiceCollection services)
        {
            return services.AddScoped<AIService>()
                .AddScoped<LicaoService>()
                .AddScoped<DevocionalDiario>()
                .AddScoped<DevocionalMulher>()
                .AddScoped<DevocionalJovem>();


        }
        public static IMcpServerBuilder AddGenAiTools(this IMcpServerBuilder mcpServerBuilder)
        {
            return mcpServerBuilder.WithTools<Tools.LicaoSkills>()
                .WithTools<Tools.DevocionalDiarioSkills>()
                .WithTools<Tools.DevocionalJovemSkills>()
                .WithTools<DevocionalMulherSkills>();

        }
    }
}
