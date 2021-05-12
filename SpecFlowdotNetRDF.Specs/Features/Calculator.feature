Feature: Calculator
![Calculator](https://specflow.org/wp-content/uploads/2020/09/calculator.png)
Simple calculator for adding **two** numbers

Link to a feature: [Calculator](SpecFlowdotNetRDF.Specs/Features/Calculator.feature)
***Further read***: **[Learn more about how to generate Living Documentation](https://docs.specflow.org/projects/specflow-livingdoc/en/latest/LivingDocGenerator/Generating-Documentation.html)**

@mytag
Scenario: Add two numbers
	Given the first number is 50
	And the second number is 70
	When the two numbers are added
	Then the result should be 120

@David
Scenario: Comparing two graphs expecting negative result
	Given There is a graph
	And The second graph is different
	When The two graphs are compared, and the result is put into a report
	Then The report should say that Graphs are not equal
	
@David
Scenario: Comparing two graphs expecting positive result
	Given There is a graph
	And The second graph is the same
	When The two graphs are compared, and the result is put into a report
	Then The report should say that Graphs are equal