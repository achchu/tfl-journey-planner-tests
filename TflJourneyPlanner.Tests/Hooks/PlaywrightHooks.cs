using Microsoft.Playwright;
using Reqnroll;
using Reqnroll.BoDi;
using TflJourneyPlanner.Tests.Support;

namespace TflJourneyPlanner.Tests.Hooks;

[Binding]
public class PlaywrightHooks
{
    private static IPlaywright _playwright = null!;
    private static IBrowser _browser = null!;
    private static readonly string ConsentStatePath =
        Path.Combine(AppContext.BaseDirectory, "consent-state.json");

    private readonly IObjectContainer _container;
    private readonly ScenarioContext _scenarioContext;
    private IBrowserContext _context = null!;

    public PlaywrightHooks(IObjectContainer container, ScenarioContext scenarioContext)
    {
        _container = container;
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static async Task LaunchBrowser()
    {
        // Results regularly took longer than Playwright's 5s default to load, which caused false failures
        Assertions.SetDefaultExpectTimeout(15000);

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = Environment.GetEnvironmentVariable("HEADED") != "1"
        });

        await SaveCookieConsentAsync();
    }

    // Dismissing the banner in every scenario was flaky: it could take over 5s to appear, and
    // #cb-cookieoverlay stayed on top of the page after dismissing it. Accepting essential cookies
    // once and reusing the saved state means the banner never shows during the tests.
    private static async Task SaveCookieConsentAsync()
    {
        var context = await _browser.NewContextAsync(new() { Locale = "en-GB" });
        var page = await context.NewPageAsync();

        await page.GotoAsync(TestSettings.JourneyPlannerUrl);
        await page.GetByRole(AriaRole.Button, new() { Name = "Accept only essential cookies" })
            .ClickAsync(new() { Timeout = 30000 });
        await page.WaitForFunctionAsync("document.cookie.includes('CookieConsent')");

        await context.StorageStateAsync(new() { Path = ConsentStatePath });
        await context.CloseAsync();
    }

    [BeforeScenario]
    public async Task CreatePage()
    {
        _context = await _browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1366, Height = 900 },
            Locale = "en-GB",
            StorageStatePath = ConsentStatePath
        });
        var page = await _context.NewPageAsync();
        page.SetDefaultTimeout(15000);

        _container.RegisterInstanceAs(page);
    }

    [AfterScenario]
    public async Task CloseContext()
    {
        if (_scenarioContext.TestError != null)
        {
            var page = _container.Resolve<IPage>();
            var fileName = $"{_scenarioContext.ScenarioInfo.Title}.png";
            foreach (var c in Path.GetInvalidFileNameChars()) fileName = fileName.Replace(c, '_');
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "screenshots", fileName);

            await page.ScreenshotAsync(new() { Path = path, FullPage = true });
            TestContext.AddTestAttachment(path);
        }

        await _context.CloseAsync();
    }

    [AfterTestRun]
    public static async Task CloseBrowser()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
}
