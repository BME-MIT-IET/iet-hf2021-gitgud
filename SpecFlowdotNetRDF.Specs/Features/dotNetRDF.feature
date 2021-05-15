Feature: dotNetRDF
	Testing dotNetRDF

@testingGraphCollections1
Scenario: Storing graph TripleStore
	Given resource loaded in graph
	Then the graph is stored
	Then the graph should be in the TripleStore

@testingGraphCollections2
Scenario: Adding graph to TripleStore based on baseURI
	Given graph is loaded from the file
	Then baseURI is loaded
	Then the graph should be added to the TripleStore

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

@Abigel
Scenario: Resolving URI with graph base
	Given a graph with a base URI
	And an URI with a path
	When creating a URI node for the graph with the same path as the other URI
	Then the URI node of the graph and the and the URI should be equal

@Abigel
Scenario: Merging two graphs
	Given a graph with 1 triple
	And an other empty graph
	When merging the two graphs
	Then the resulting graph should have 1 triple