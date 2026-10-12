Feature: Plan a journey
    As a TfL customer
    I want to plan a journey between two stations using the Journey Planner
    So that I can choose the best way to travel

    Background:
        Given I am on the TfL Journey Planner
        When I select "Leicester Square Underground Station" as the origin by typing "Leicester Sq"
        And I select "Covent Garden Underground Station" as the destination by typing "Covent Gar"
        And I plan the journey

    @smoke
    Scenario: A valid journey can be planned using autocomplete suggestions
        Then the journey results should be displayed
        And the origin should be "Leicester Square Underground Station"
        And the destination should be "Covent Garden Underground Station"

    Scenario: Walking and cycling times are shown for a valid journey
        Then a walking time should be displayed in minutes
        And a cycling time should be displayed in minutes

    Scenario: Journey time is shown after choosing routes with least walking
        When I edit preferences to show routes with least walking
        And I update the journey
        Then the journey preferences should show least walking
        And a journey time should be displayed in minutes

    Scenario: Access information is shown for Covent Garden in the journey details
        When I edit preferences to show routes with least walking
        And I update the journey
        And I view the details of the first journey
        Then access information for "Covent Garden Underground Station" should be displayed