using Reqnroll;
using TflJourneyPlanner.Tests.Pages;
using static Microsoft.Playwright.Assertions;

namespace TflJourneyPlanner.Tests.StepDefinitions;

[Binding]
public class JourneyPlannerSteps
{
    private readonly JourneyPlannerPage _planner;

    public JourneyPlannerSteps(JourneyPlannerPage planner) => _planner = planner;

    [Given("I am on the TfL Journey Planner")]
    public Task GivenIAmOnTheJourneyPlanner() => _planner.OpenAsync();

    [When("I select {string} as the origin by typing {string}")]
    public Task WhenISelectTheOrigin(string suggestion, string typedText) =>
        _planner.SelectOriginAsync(typedText, suggestion);

    [When("I select {string} as the destination by typing {string}")]
    public Task WhenISelectTheDestination(string suggestion, string typedText) =>
        _planner.SelectDestinationAsync(typedText, suggestion);

    [When("I enter {string} as the origin without selecting a suggestion")]
    public Task WhenIEnterTheOrigin(string text) => _planner.EnterOriginAsync(text);

    [When("I enter {string} as the destination without selecting a suggestion")]
    public Task WhenIEnterTheDestination(string text) => _planner.EnterDestinationAsync(text);

    [When("I plan the journey")]
    public Task WhenIPlanTheJourney() => _planner.PlanJourneyAsync();

    [Then("I should see a validation error for the origin")]
    public Task ThenIShouldSeeOriginError() =>
        Expect(_planner.FromError).ToHaveTextAsync("The From field is required.");

    [Then("I should see a validation error for the destination")]
    public Task ThenIShouldSeeDestinationError() =>
        Expect(_planner.ToError).ToHaveTextAsync("The To field is required.");

    [Then("I should remain on the Journey Planner page")]
    public void ThenIShouldRemainOnThePlannerPage() =>
        Assert.That(_planner.CurrentUrl, Does.Not.Contain("/results"));
}
