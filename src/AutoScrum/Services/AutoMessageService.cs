namespace AutoScrum.Services;

public class AutoMessageService
{
    private readonly MessageService _messageService;

    public AutoMessageService(MessageService messageService)
    {
        _messageService = messageService;
    }

    public void Success(string message) => _messageService.Success(message);

    public void Warning(string message) => _messageService.Warning(message);

    public void Error(string message) => _messageService.Error(message);
}