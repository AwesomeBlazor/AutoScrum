using Microsoft.JSInterop;

namespace AutoScrum.Services;

public interface IClipboardService
{
    Task Copy(string text, string html);
}

public class ClipboardService : IClipboardService
{
    private readonly IJSRuntime _jSRuntime;

    public ClipboardService(IJSRuntime jSRuntime)
    {
        _jSRuntime = jSRuntime;
    }

    public async Task Copy(string text, string html)
    {
        await _jSRuntime.InvokeVoidAsync("setClipboard", text, html);
    }
}
