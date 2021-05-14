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