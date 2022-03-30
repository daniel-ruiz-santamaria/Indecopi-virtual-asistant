Feature: SpecFlowFeature1
	Login to EA Demo application

@smoke
Scenario: Perform Login of EA Application site
	Given I launch the application
	And I click login link
	And I enter the following details
		| UserName | Password |
		| admin	   | password |
	And I click login button
	Then I should see Employee details link