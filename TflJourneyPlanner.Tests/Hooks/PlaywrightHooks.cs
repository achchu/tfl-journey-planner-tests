using Microsoft.Playwright;
using Reqnroll;
using Reqnroll.BoDi;

namespace TflJourneyPlanner.Tests.Hooks;

[Binding]
public class PlaywrightHooks
{
    private static IPlaywright _playwright = null!;
    private static IBrowser _browser = null!;

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
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new()
        {
            Headless = Environment.GetEnvironmentVariable("HEADED") != "1"
        });
    }

    [BeforeScenario]
    public async Task CreatePage()
    {
        _context = await _browser.NewContextAsync(new()
        {
            ViewportSize = new() { Width = 1366, Height = 900 },
            Locale = "en-GB"
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