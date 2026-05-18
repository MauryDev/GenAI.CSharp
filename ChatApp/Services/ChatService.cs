using ChatApp.Models;
using ChatApp.Repositories;
using GenAI.CSharp.Services;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace ChatApp.Services;

public class ChatService
{
    private readonly AIService _aiService;
    private readonly ChatRepository _repository;
    private readonly ChatStateContainer _state;

    public event Action? OnMessagesChanged;

    public ChatService(AIService aiService, ChatRepository repository, ChatStateContainer state)
    {

      
        _aiService = aiService;
        _repository = repository;
        _state = state;
    }

   
    public async Task<IEnumerable<Models.ChatMessage>> GetMessagesAsync()
    {
        if (_state.CurrentChatId == null)
            return Enumerable.Empty<Models.ChatMessage>();

        var session = await _repository.GetSessionWithMessagesAsync(_state.CurrentChatId.Value);
        return session?.Messages ?? Enumerable.Empty<Models.ChatMessage>();
    }

    public async Task<string> ProcessMessageAsync(string userMessage)
    {
        var sessionId = await EnsureActiveSession();
        await _repository.AddMessageAsync(sessionId, "user", userMessage);

        try
        {
            var history = await GetChatHistory(sessionId);
            var response = await _aiService.ChatAsync(history);
            
            await _repository.AddMessageAsync(sessionId, "assistant", response);
            await TryGenerateTitle(sessionId, userMessage);
            
            NotifyChanges();
            return response;
        }
        catch (Exception ex)
        {
            return await HandleError(sessionId, ex);
        }
    }

    private async Task<Guid> EnsureActiveSession()
    {
        if (_state.CurrentChatId == null)
        {
            var session = await _repository.CreateSessionAsync();
            _state.SetCurrentChatId(session.Id);
        }
        return _state.CurrentChatId!.Value;
    }

    private async Task<List<Microsoft.Extensions.AI.ChatMessage>> GetChatHistory(Guid sessionId)
    {
        var session = await _repository.GetSessionWithMessagesAsync(sessionId);
        return session?.Messages.Select(MapToAiMessage).ToList() ?? new();
    }

    private Microsoft.Extensions.AI.ChatMessage MapToAiMessage(Models.ChatMessage message)
    {
        var role = message.Role switch
        {
            "user" => ChatRole.User,
            "assistant" => ChatRole.Assistant,
            "system" => ChatRole.System,
            _ => ChatRole.User
        };
        return new Microsoft.Extensions.AI.ChatMessage(role, message.Content);
    }

    private async Task TryGenerateTitle(Guid sessionId, string firstMessage)
    {
        var session = await _repository.GetSessionWithMessagesAsync(sessionId);
        if (session?.Messages.Count(m => m.Role == "user") == 1)
        {
            var prompt = new List<Microsoft.Extensions.AI.ChatMessage>
            {
                new(ChatRole.System, "Generate a short, descriptive chat title (max 30 chars). Return ONLY the title text."),
                new(ChatRole.User, firstMessage)
            };
            var title = await _aiService.ChatAsync(prompt);
            await _repository.UpdateSessionTitleAsync(sessionId, title.Trim());
        }
    }

    private async Task<string> HandleError(Guid sessionId, Exception ex)
    {
        var message = $"Error: {ex.Message}";
        await _repository.AddMessageAsync(sessionId, "assistant", message);
        NotifyChanges();
        return message;
    }

    private void NotifyChanges() => OnMessagesChanged?.Invoke();

    public async Task CreateNewSession()
    {
        var session = await _repository.CreateSessionAsync();
        _state.SetCurrentChatId(session.Id);
        NotifyChanges();
    }
}
