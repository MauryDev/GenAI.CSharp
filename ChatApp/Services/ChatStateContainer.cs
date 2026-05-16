namespace ChatApp.Services;

public class ChatStateContainer
{
    public Guid? CurrentChatId { get; private set; }
    public event Action? OnChange;

    public void SetCurrentChatId(Guid? chatId)
    {
        CurrentChatId = chatId;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
