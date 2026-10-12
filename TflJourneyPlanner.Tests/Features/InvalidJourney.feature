Feature: Journey Planner handles invalid input
  As a TfL customer
  I want clear feedback when my journey cannot be planned
  So that I understand what I need to correct

  Background:
    Given I am on the TfL Journey Planner

  @negative
  Scenario Outline: No results are returned when one or more locations are invalid
    When I enter "<from>" as the origin without selecting a suggestion
    And I enter "<to>" as the destination without selecting a suggestion
    And I plan the journey
    Then no journey results should be displayed

    Examples:
      | case             | from                                 | to                                |
      | invalid origin   | qzxjvwk                              | Covent Garden Underground Station |
      | invalid dest     | Leicester Square Underground Station | qzxjvwk                           |
      | both invalid     | qzxjvwk                              | qzxjvwkq                          |

  @negative
  Scenario: A journey cannot be planned when no locations are entered
    When I plan the journey
    Then I should see a validation error for the origin
    And I should see a validation error for the destination
    And I should remain on the Journey Planner page
