using GenAI.CSharp.Utils;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Diagnostics;
using System.Text.Json;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace GenAI.CSharp.Services;

public class AIService
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<AIService> _logger;
    private readonly List<AITool> _tools = [];

    public AIService(IEnumerable<ISkillsService> skillsServices, IConfiguration config, ILogger<AIService> logger)
    {
        _logger = logger;
        var tokenAI = config["AIConfig:PROVIDER_TOKEN"]!;
        var modelName = config["AIConfig:MODEL_NAME"]!;

        var providerUrl = new Uri(config["AIConfig:PROVIDER_URL"]!);



        var openAIClientConfiguration = new OpenAIClientOptions();

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(tokenAI),
            new OpenAIClientOptions { Endpoint = providerUrl }
        );

        _chatClient = openAIClient
            .GetChatClient(modelName)
            .AsIChatClient();


        RegisterTools();

        AddTools(skillsServices.Select(skillsServices => skillsServices.Skills));
    }



    private void RegisterTools()
    {

        _tools.Add(
            AIFunctionFactory.Create(
                async () => DateTime.Now.ToString("dddd"),
                name: "diaSemanaHoje",
                "Retorna o dia da semana atual."
            )
        );
    }

    public void AddTools(IEnumerable<AITool> aITools)
    {
        _tools.AddRange(aITools);
    }
    public void AddTools(IEnumerable<ISkills> aITools)
    {
        _tools.AddRange(aITools.SelectMany(e => e.GetAITools()));
    }
    public void AddTools(ISkills aITools)
    {
        _tools.AddRange(aITools.GetAITools());
    }
    public void AddTool(AITool aITool)
    {
        _tools.Add(aITool);
    }

    public async Task<string> ChatAsync(List<ChatMessage> messages)
    {
        var options = new ChatOptions
        {
            Tools = _tools,
            ToolMode = ChatToolMode.Auto
        };

        while (true)
        {
            var sw = Stopwatch.StartNew();
            var response = await _chatClient.GetResponseAsync(messages, options);
            sw.Stop();
            
            _logger.LogInformation("AI response received in {Elapsed}ms", sw.ElapsedMilliseconds);

            var message = response.Messages.First();
            messages.Add(message);

            var toolCalls = message.Contents.OfType<FunctionCallContent>().ToList();
            if (toolCalls.Count == 0) return message.Text ?? "";

            foreach (var toolCall in toolCalls)
            {
                var tool = _tools.OfType<AIFunction>().FirstOrDefault(t => t.Name == toolCall.Name);
                if (tool != null)
                {
                    _logger.LogInformation("Tool: Calling {ToolName}", toolCall.Name);

                    var toolSw = Stopwatch.StartNew();
                    var result = await tool.InvokeAsync(new(toolCall.Arguments));
                    toolSw.Stop();

                    _logger.LogInformation("Tool {ToolName} executed in {Elapsed}ms", toolCall.Name, toolSw.ElapsedMilliseconds);

                    messages.Add(new ChatMessage(ChatRole.Tool, JsonSerializer.Serialize(result))
                    {
                        Contents = { new FunctionResultContent(toolCall.CallId, result) }
                    });
                }
            }
        }
    }
}