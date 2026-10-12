namespace TflJourneyPlanner.Tests.Support;

public static class TestSettings
{
    // Keep the trailing slash - without it TfL intermittently returned an empty 200 response in my runs
    public const string JourneyPlannerUrl = "https://tfl.gov.uk/plan-a-journey/";
}
