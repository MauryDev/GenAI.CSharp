using GenAI.CSharp.Services;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace GenAI.CSharp.Utils
{
    public static class AIToolHelper
    {

        extension (ISkills skills)
        {
            public IEnumerable<AITool> GetAITools()
            {
                var methods = skills
                    .GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (var method in methods)
                {
                    // Cria a função dinamicamente mantendo o nome do método e os metadados
                    yield return AIFunctionFactory.Create(method, skills);
                }
            }
        }
    }
}
