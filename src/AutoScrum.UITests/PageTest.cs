namespace AutoScrum.UITests;

public abstract class PageTest : IAsyncLifetime
{
#if DEBUG
    public const string BaseUrl = "https://autoscrum.jkdev.me/";
#else
    public const string BaseUrl = "https://autoscrum.jkdev.me/";
#endif

    public IPlaywright PlaywrightInstance { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public IBrowserContext BrowserContext { get; private set; } = null!;
    public IPage Page { get; private set; } = null!;

    public virtual float? SlowMo { get; protected set; }

    public async Task InitializeAsync()
    {
        PlaywrightInstance = await Playwright.CreateAsync();
        Browser = await PlaywrightInstance.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
#if DEBUG
            Headless = false,
            SlowMo = SlowMo
#else
            Headless = true,
            SlowMo = null
#endif
        });

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
