using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace TflJourneyPlanner.Tests.Pages;

public class JourneyResultsPage
{
    private readonly IPage _page;

    public JourneyResultsPage(IPage page) => _page = page;

    public ILocator Summary => _page.Locator(".journey-result-summary");
    public ILocator Origin => Summary.Locator(".summary-row").Filter(new() { HasText = "From:" });
    public ILocator Destination => Summary.Locator(".summary-row").Filter(new() { HasText = "To:" });

    public ILocator WalkingBox => _page.Locator(".journey-box.walking");
    public ILocator CyclingBox => _page.Locator(".journey-box.cycling");
    public ILocator TravelPreferences => _page.Locator(".travel-preferences");
    public ILocator LeastWalkingOption => _page.GetByLabel("Routes with least walking");

    public ILocator FirstJourneyTime =>
        _page.Locator(".publictransport-box .journey-time").Filter(new() { Visible = true }).First;

    public ILocator JourneyOptions =>
        _page.Locator(".journey-box, .publictransport-box").Filter(new() { Visible = true });

    public ILocator NoResultsMessage => _page.Locator(".ajax-response .field-validation-error");

    public Task WaitForResultsPageAsync() => _page.WaitForURLAsync("**/plan-a-journey/results**");

    public Task EditPreferencesAsync() =>
        _page.GetByRole(AriaRole.Button, new() { Name = "Edit preferences" }).ClickAsync();

    // The styled label sits on top of the radio input and intercepts clicks, so click the label like a user
    public Task ChooseLeastWalkingAsync() =>
        _page.GetByText("Routes with least walking", new() { Exact = true }).ClickAsync();

    public Task UpdateJourneyAsync() =>
        _page.GetByRole(AriaRole.Button, new() { Name = "Update journey" }).ClickAsync();

    // "View details" buttons are aria-hidden (logged as an accessibility issue), so GetByRole can't find them
    public Task ViewFirstJourneyDetailsAsync() =>
        _page.Locator(".publictransport-box button.view-hide-details")
            .Filter(new() { Visible = true })
            .First
            .ClickAsync();

    public ILocator AccessInformationFor(string station) =>
        _page.Locator(".journey-detail-step")
            .Filter(new() { HasTextRegex = new Regex($@"Depart\s+{Regex.Escape(station)}"), Visible = true })
            .Locator(".access-information .tooltip");
}
