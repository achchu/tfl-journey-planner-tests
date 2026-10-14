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
- PowerShell (`pwsh`) – used once to install Playwright's browser. If it isn't installed: `dotnet tool install --global PowerShell`

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

## Latest run

All 8 tests pass in roughly 30 seconds, headless. Values observed on the live site at the time of writing (written to the test output on each run):

- Walking: 6 mins, cycling: 1 min
- Least walking route: Piccadilly line, 11 mins total
- Access information at Covent Garden Underground Station: Up stairs, Up lift, Level walkway

## Design decisions

**Playwright instead of ChromeDriver.** The brief mentions both Playwright and ChromeDriver. ChromeDriver is the Selenium driver for Chrome; Playwright drives Chromium directly and doesn't use it. I chose Playwright for its built-in auto-waiting and retrying assertions, which matter on a site with asynchronous results like this one.

**Page Object Model.** Step definitions contain no selectors. They call methods on `JourneyPlannerPage` and `JourneyResultsPage`. If TfL changes its markup, only the page objects need updating. Role-based locators (`GetByRole`, `GetByLabel`) are used where the markup supports them; IDs/CSS are used where it doesn't.

**Gherkin with Reqnroll.** Reqnroll is the actively maintained successor to SpecFlow. Steps are written from the user's point of view ("I plan the journey") rather than as UI clicks, so the feature files read as living documentation that non-technical stakeholders can review.

**Reusable, parameterised steps.** Quoted values in the Gherkin become step parameters (Cucumber expressions such as `{string}`), so one step definition handles any station. The invalid-location test is a Scenario Outline with an Examples table, giving three tests from one scenario.

**Background for shared setup.** All four happy-path scenarios need a planned journey, so it lives in the `Background:`. Each scenario still runs independently because the Background runs before every scenario.

**Selecting from autocomplete, not typing the full name.** As the brief requires, the tests type a partial name ("Leicester Sq") and select the exact suggestion ("Leicester Square Underground Station"). Typing is done key by key because the autocomplete reacts to keystrokes. For the invalid tests, valid stations are typed in full without selecting a suggestion, so only one thing is wrong in each example.

**Test isolation.** The browser is launched once per run, but every scenario gets a fresh browser context with clean cookies and storage, so scenarios don't depend on each other and can run in any order.

**Dependency injection.** Reqnroll's container provides each scenario with its own `IPage`. Page objects and step classes receive it through their constructors, so there is no shared global state.

**Cookie consent handled once per run.** Dismissing the Cookiebot banner inside each scenario was flaky: on a fresh browser it could take over 5 seconds to appear, and after dismissing it the `#cb-cookieoverlay` element stayed on top of the page and intercepted clicks. Instead, the hooks accept essential cookies once, save the browser storage state, and start every scenario with it, so the banner never appears during the tests. I chose "Accept only essential cookies" as the privacy-preserving option.

**Assertions on live data.** Journey times change throughout the day, so the tests assert a realistic number of minutes (1–60) instead of exact values. The actual values are written to the test output for traceability.

**Avoiding false passes on "no results".** Asserting zero results on its own could pass before the results had loaded. The negative test first waits for TfL's "Sorry, we can't find a journey matching your criteria" message, then asserts that no journey options are shown.

**Least walking is needed for the access-information scenario.** For this short journey TfL's default results only offer walking and cycling ("If you can, walk or cycle this route"). Choosing "Routes with least walking" brings up a Piccadilly line option, and its details include the step-by-step access information for Covent Garden.

**Invalid test data.** TfL fuzzy-matches many nonsense strings to real places. My first choice, `Xyzzyplace123`, was matched to "Mayplace" and offered a list of locations, and `xqzjkv` even returned real journeys. I checked candidates against the live site and used `qzxjvwk` and `qzxjvwkq`, which consistently return "Sorry, we can't find a journey".

**Timeouts.** Playwright's default assertion timeout is 5 seconds. TfL's results regularly took longer, which caused false failures, so the assertion and action timeouts are set to 15 seconds.

**Planner URL with a trailing slash.** The tests open `/plan-a-journey/` rather than `/plan-a-journey`, because the address without the slash intermittently returned an empty page during my runs (see D1 below).

**Screenshot on failure.** When a scenario fails, a full-page screenshot is saved and attached to the NUnit result, which makes failures quicker to diagnose locally and in CI.

**Headless by default.** Tests run headless, as they would in a pipeline. Setting `HEADED=1` shows the browser for local debugging.

## Defects and observations

Found while building and running the tests. D1 and D2 were observed in Playwright-driven Chromium; I have not confirmed whether they affect users in a normal browser.

| ID | Summary | Steps to reproduce | Expected | Actual | Severity |
|---|---|---|---|---|---|
| D1 | Planner URL without a trailing slash intermittently returns an empty page | Open `https://tfl.gov.uk/plan-a-journey` repeatedly in a fresh browser | 301 redirect to `/plan-a-journey/` and the planner loads | Some requests return 200 with an empty body, so the page is blank | Medium |
| D2 | Cookie overlay stays on the page after the banner is dismissed | Fresh browser → open the Journey Planner → click "Accept only essential cookies" → click "Plan my journey" | Banner and overlay are removed and the page can be used | Banner closes, consent is saved, but `#cb-cookieoverlay` stays visible and intercepts clicks | Medium |
| D3 | "View details" buttons are hidden from assistive technology | Plan a public transport journey → inspect a "View details" button | Visible, interactive buttons are exposed to screen readers | The buttons have `aria-hidden="true"` while visible and clickable | Medium |
| O1 | Nonsense input is matched to real places | Enter `xqzjkv` as the destination and plan the journey | "Sorry, we can't find a journey" or a list of possible matches | Real journeys are returned for an unrecognisable destination | Low (observation) |

## Additional test scenarios (not automated)

### Functional
- Swap origin and destination and check the results update
- "Leaving" vs "Arriving" at a specific date and time
- Planning a journey for a date or time in the past shows a warning
- Adding a "via" location includes it in the route
- Same origin and destination
- Postcodes, landmarks and street addresses as input
- Choosing an option from "We found more than one location matching…" plans the journey
- Mode filters (e.g. excluding buses) are reflected in the results
- Accessibility preferences (step-free to platform / to vehicle) change the route offered
- Walking speed and cycling preference options change the times shown
- "Earlier journeys" and "Later journeys" load the correct time windows
- "Edit journey" keeps the previously entered values
- Results page opened from a shared URL / deep link
- Browser back and forward navigation from the results page
- Recent searches and favourites work when functional cookies are accepted, and are disabled when they are not

### Accessibility
- Automated scans of the planner and results pages with axe-core / Accessibility Insights
- Keyboard-only use: tab order, operating the autocomplete with arrow keys and Enter, submitting the form
- Screen reader announcements for autocomplete suggestions, validation errors and loaded results
- Focus moves sensibly after submitting the form and after a validation error
- Colour contrast of text, buttons and line colours
- Zoom to 200% and 400%, and reflow at 320px width without horizontal scrolling

### Performance
- Response time for autocomplete suggestions while typing
- Time to load the results page and the first journey option
- Load test of the journey planning API (e.g. JMeter / k6) at expected peak volumes, such as rush hour or during disruption
- Behaviour under a slow network (throttled 3G)

### Compatibility
- Chromium, Firefox and WebKit/Safari
- Mobile and tablet viewports, including touch interaction with the autocomplete
- Different browser language/locale settings

### Security
- Script or HTML injection in the location fields is not executed or rendered
- Very long input and special characters are handled safely
- No sensitive data in URLs or query strings beyond the journey parameters

### Resilience
- A clear error is shown when the journey API is slow, returns an error or times out
- Behaviour when the connection drops mid-search
- Journeys during planned closures or live disruption show the relevant alerts

### API
- Contract and response tests for the TfL Unified API `/Journey/JourneyResults` endpoint (e.g. Postman), including invalid locations and missing parameters

## Possible improvements

- Run the suite against Firefox and WebKit as well as Chromium
- CI pipeline (GitHub Actions or Azure DevOps) running the tests on every push and pull request, publishing screenshots as artifacts
- HTML reporting (e.g. Allure) and Playwright traces on failure
- Run scenarios in parallel to reduce run time
- Move settings (base URL, browser, headless, timeouts) into `appsettings.json` / environment variables to target different environments
- Automated accessibility checks with axe-core (Deque.AxeCore.Playwright) on the planner and results pages
- API-level tests for the journey planning endpoint to cover more data combinations quickly

## Use of AI

I've been working with TypeScript in my career, so C# was new to me. I used AI as an assistant throughout this project:

- **Code:** AI helped write the C# code, and explained C# syntax and .NET conventions such as file naming and dependency injection.
- **Review:** I used AI to review my code and this README.

I designed the test scenarios, chose the approach, ran and debugged the tests against the live site, and decided which issues to report.
