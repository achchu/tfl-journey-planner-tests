using Microsoft.Playwright;
using TflJourneyPlanner.Tests.Support;

namespace TflJourneyPlanner.Tests.Pages;

public class JourneyPlannerPage
{
    private readonly IPage _page;

    public JourneyPlannerPage(IPage page) => _page = page;

    private ILocator FromInput => _page.Locator("#InputFrom");
    private ILocator ToInput => _page.Locator("#InputTo");
    private ILocator PlanJourneyButton => _page.Locator("#plan-journey-button");

    public ILocator FromError => _page.Locator("#InputFrom-error");
    public ILocator ToError => _page.Locator("#InputTo-error");
    public string CurrentUrl => _page.Url;

    public async Task OpenAsync()
    {
        await _page.GotoAsync(TestSettings.JourneyPlannerUrl);
    }

    public Task SelectOriginAsync(string typedText, string suggestion) =>
        SelectSuggestionAsync(FromInput, "#InputFrom-dropdown", typedText, suggestion);

    public Task SelectDestinationAsync(string typedText, string suggestion) =>
        SelectSuggestionAsync(ToInput, "#InputTo-dropdown", typedText, suggestion);

    public Task EnterOriginAsync(string text) => EnterWithoutSelectingAsync(FromInput, text);

    public Task EnterDestinationAsync(string text) => EnterWithoutSelectingAsync(ToInput, text);

    public Task PlanJourneyAsync() => PlanJourneyButton.ClickAsync();

    private async Task SelectSuggestionAsync(ILocator input, string dropdown, string typedText, string suggestion)
    {
        await input.ClickAsync();
        await input.PressSequentiallyAsync(typedText, new() { Delay = 50 });
        await _page.Locator(dropdown)
            .GetByRole(AriaRole.Option, new() { Name = suggestion, Exact = true })
            .ClickAsync();
    }

    private async Task EnterWithoutSelectingAsync(ILocator input, string text)
    {
        await input.FillAsync(text);
        await input.PressAsync("Escape");
    }
}
