using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenAI.CSharp.Services
{
    public static class MainService
    {
       
        public static void AddGenAiLesson(this IServiceCollection services)
        {
            services.AddScoped<AIService>();
            services.AddScoped<LicaoService>();

        }
        public static void AddGenAiTools(this IMcpServerBuilder mcpServerBuilder)
        {
            mcpServerBuilder.WithTools<Tools.LicaoSkills>();
        }
    }
}
