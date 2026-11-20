Feature: Fast-food shopping cart

Background:
    Given the shop time is "2026-11-07T12:00"
    And the standard menu is available

Scenario: Order burgers and fries during opening hours
    When I add 2 "Burger"
    And I add 1 "Fries"
    And I checkout
    Then the operation should succeed
    And the receipt total should be "14.60" EUR
    And the receipt ready time should be "12:08"
    And the cart should be empty

Scenario: Order a shake during opening hours
    When I add 1 "Shake"
    And I checkout
    Then the operation should succeed
    And the receipt total should be "3.50" EUR
    And the receipt ready time should be "12:04"
    And the cart should be empty

Scenario: Unknown product is rejected
    When I add 1 "Pizza"
    Then the operation should fail with message containing "not on the menu"

Scenario: Remove a product from the cart
    When I add 1 "Burger"
    And I remove "Burger"
    Then the cart should be empty

Scenario Outline: Invalid quantity text is rejected
    When I add "<quantity>" "<product>" as text
    Then the operation should fail with message containing "quantity"

Examples:
    | quantity | product |
    | 0        | Burger  |
    | -1       | Burger  |
    | abc      | Burger  |

Scenario: Checkout outside opening hours is rejected
    Given the shop time is "2026-11-07T23:00"
    When I add 1 "Burger"
    And I checkout
    Then the operation should fail with message containing "closed"

Scenario: Checkout on Sunday is rejected
    Given the shop time is "2026-11-08T12:00"
    When I add 1 "Burger"
    And I checkout
    Then the operation should fail with message containing "closed"