namespace AutoScrum.UITests;

public abstract class PageTest : IAsyncLifetime
{
    public IPlaywright PlaywrightInstance { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public IBrowserContext BrowserContext { get; private set; } = null!;
    public IPage Page { get; private set; } = null!;

    public string BaseUrl { get; private set; } = "https://localhost:5001";

    public virtual float? SlowMo { get; protected set; }

    public async Task InitializeAsync()
    {
        var browserOptions = new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = SlowMo
        };

        // Add support to modify BaseUrl from outside.
        string? isGitHubActions = Environment.GetEnvironmentVariable("IsGitHubActions");
        string? baseUrl = Environment.GetEnvironmentVariable("BaseUrl");
        if (isGitHubActions == "true")
        {
            // Assume it's in GitHub actions and we want to run it fast.
            browserOptions.Headless = true;
            browserOptions.SlowMo = null;
            baseUrl ??= "https://autoscrum.jkdev.me";
        }

        if (!string.IsNullOrWhiteSpace(baseUrl))
        {
            BaseUrl = baseUrl;
        }

        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(browserOptions);

        BrowserContext = await Browser.NewContextAsync();
        Page = await BrowserContext.NewPageAsync();

        await Page.GotoAsync(BaseUrl);
    }

    public async Task DisposeAsync()
    {
        if (BrowserContext != null)
        {
            await BrowserContext.DisposeAsync();
        }

        PlaywrightInstance?.Dispose();
    }
}
