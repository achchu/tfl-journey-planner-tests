namespace TflJourneyPlanner.Tests.Support;

public static class TestSettings
{
    // Trailing slash matters: without it TfL intermittently returns an empty page
    public const string JourneyPlannerUrl = "https://tfl.gov.uk/plan-a-journey/";
}
