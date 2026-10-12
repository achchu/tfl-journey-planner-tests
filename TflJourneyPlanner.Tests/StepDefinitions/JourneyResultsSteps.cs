using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Reqnroll;
using TflJourneyPlanner.Tests.Pages;
using static Microsoft.Playwright.Assertions;

namespace TflJourneyPlanner.Tests.StepDefinitions;

[Binding]
public class JourneyResultsSteps
{
    private static readonly Regex Minutes = new(@"(\d+)\s*mins?");

    private readonly JourneyResultsPage _results;
    private readonly IReqnrollOutputHelper _output;

    public JourneyResultsSteps(JourneyResultsPage results, IReqnrollOutputHelper output)
    {
        _results = results;
        _output = output;
    }

    [Then("the journey results should be displayed")]
    public async Task ThenTheJourneyResultsShouldBeDisplayed()
    {
        await _results.WaitForResultsPageAsync();
        await Expect(_results.Summary).ToBeVisibleAsync();
    }

    [Then("the origin should be {string}")]
    public Task ThenTheOriginShouldBe(string station) =>
        Expect(_results.Origin).ToContainTextAsync(station);

    [Then("the destination should be {string}")]
    public Task ThenTheDestinationShouldBe(string station) =>
        Expect(_results.Destination).ToContainTextAsync(station);

    [Then("a walking time should be displayed in minutes")]
    public Task ThenAWalkingTimeShouldBeDisplayed() => AssertMinutesAsync(_results.WalkingBox, "Walking");

    [Then("a cycling time should be displayed in minutes")]
    public Task ThenACyclingTimeShouldBeDisplayed() => AssertMinutesAsync(_results.CyclingBox, "Cycling");

    [When("I edit preferences to show routes with least walking")]
    public async Task WhenIEditPreferencesToShowLeastWalking()
    {
        await _results.EditPreferencesAsync();
        await _results.ChooseLeastWalkingAsync();
        await Expect(_results.LeastWalkingOption).ToBeCheckedAsync();
    }

    [When("I update the journey")]
    public Task WhenIUpdateTheJourney() => _results.UpdateJourneyAsync();

    [Then("the journey preferences should show least walking")]
    public Task ThenThePreferencesShouldShowLeastWalking() =>
        Expect(_results.TravelPreferences).ToContainTextAsync("Showing routes with least walking");

    [Then("a journey time should be displayed in minutes")]
    public Task ThenAJourneyTimeShouldBeDisplayed() => AssertMinutesAsync(_results.FirstJourneyTime, "Journey");

    [When("I view the details of the first journey")]
    public Task WhenIViewTheDetailsOfTheFirstJourney() => _results.ViewFirstJourneyDetailsAsync();

    [Then("access information for {string} should be displayed")]
    public async Task ThenAccessInformationShouldBeDisplayed(string station)
    {
        var items = _results.AccessInformationFor(station);
        await Expect(items.First).ToBeVisibleAsync();

        var descriptions = (await items.AllTextContentsAsync()).Select(d => d.Trim()).ToList();
        _output.WriteLine($"Access information at {station}: {string.Join(", ", descriptions)}");

        Assert.That(descriptions, Has.All.Not.Empty, "Every access item should have a description");
    }

    [Then("no journey results should be displayed")]
    public async Task ThenNoJourneyResultsShouldBeDisplayed()
    {
        // Check for TfL's message first - asserting zero results on its own could pass before results had loaded
        await Expect(_results.NoResultsMessage).ToHaveTextAsync("Sorry, we can't find a journey matching your criteria");
        await Expect(_results.JourneyOptions).ToHaveCountAsync(0);
    }

    // Journey times are live, so I check for a realistic value rather than an exact one
    private async Task AssertMinutesAsync(ILocator locator, string label)
    {
        await Expect(locator).ToContainTextAsync(Minutes);
        var minutes = int.Parse(Minutes.Match(await locator.InnerTextAsync()).Groups[1].Value);
        _output.WriteLine($"{label} time: {minutes} mins");

        Assert.That(minutes, Is.InRange(1, 60), $"{label} time should be a sensible number of minutes");
    }
}
