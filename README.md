# TfL Journey Planner: UI Test Automation

Automated UI tests for the [TfL Journey Planner](https://tfl.gov.uk/plan-a-journey) widget, written in C# with Gherkin (Reqnroll) and Playwright.

## Tech stack

| Tool | Version | Purpose |
|---|---|---|
| .NET | 10 | Runtime (current LTS) |
| Reqnroll | 3.3 | Gherkin / BDD |
| NUnit | 4.3 | Test runner |
| Microsoft.Playwright | 1.63 | Browser automation |

## Running the tests

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PowerShell (`pwsh`) – used once to install Playwright's browser

### Setup
```bash
git clone https://github.com/achchu/tfl-journey-planner-tests.git
cd tfl-journey-planner-tests
dotnet build
pwsh TflJourneyPlanner.Tests/bin/Debug/net10.0/playwright.ps1 install chromium
```

### Run
```bash
dotnet test                                   # all tests, headless
HEADED=1 dotnet test                          # watch the browser
dotnet test --filter "Category=smoke"         # smoke test only
dotnet test --filter "Category=negative"      # negative tests only
```

On failure, a full-page screenshot is saved to `bin/Debug/net10.0/screenshots/` and attached to the test result.

## Project structure

```
TflJourneyPlanner.Tests/
├── Features/          Gherkin scenarios (what is tested)
├── StepDefinitions/   Binds each Gherkin step to page object calls
├── Pages/             Page objects – all selectors and page interactions live here
├── Hooks/             Browser lifecycle, cookie consent, screenshot on failure
└── Support/           Shared settings
```

## Design decisions

**Playwright instead of ChromeDriver.** The brief mentions both Playwright and ChromeDriver. ChromeDriver is the Selenium driver for Chrome; Playwright drives Chromium directly and doesn't use it. I chose Playwright for its built-in auto-waiting and retrying assertions, which matter on a site with asynchronous results like this one.

**Page Object Model.** Step definitions contain no selectors. They call methods on `JourneyPlannerPage` and `JourneyResultsPage`. If TfL changes its markup, only the page objects need updating. Role-based locators (`GetByRole`, `GetByLabel`) are used where the markup supports them; IDs/CSS are used where it doesn't.

## Use of AI

I've been working with typescript in my career so C# was new to me, I used AI to help me with the syntax, file naming convention and so on. I also used it to do a code review of my code where required
