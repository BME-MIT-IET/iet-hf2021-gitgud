using FluentAssertions;
using System;
using System.IO;
using TechTalk.SpecFlow;
using VDS.RDF;

namespace SpecFlowdotNetRDF.Specs.Steps
{
    [Binding]
    public sealed class GraphCollectionSteps
    {
        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioContext;
        private TripleStore triplestore = new TripleStore();
        private TripleStore triplestore2 = new TripleStore(new WebDemandGraphCollection());
        private Graph graph = new Graph();
        private Graph graph2 = new Graph();
        private bool isadded;
        private bool isthesecondadded = false;

        GraphCollection _graphcollection = new GraphCollection();

        public GraphCollectionSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given("resource loaded in graph")]
        public void GivenTheResourceIsLoaded()
        {
            graph.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
        }

        [Then("the graph is stored")]
        public void ThenAddedToStrore()
        {
            triplestore.Add(graph);
            isadded = triplestore.HasGraph(graph.BaseUri);
        }

        [Then("the graph should be in the TripleStore")]
        public void ThenTheResultShouldBe()
        {
            isadded.Should().Be(true);
        }

        [Given("graph is loaded from the file")]
        public void GivenTheGraphIsLoaded()
        {
            graph2.LoadFromFile("resources\\InferenceTest.ttl");
        }

        [Then("baseURI is loaded")]
        public void ThenTheBaseURIIsLoaded()
        {
            graph2.BaseUri = new Uri("file:///" + Path.GetFullPath("resources\\InferenceTest.ttl"));
            isthesecondadded = triplestore2.HasGraph(graph2.BaseUri);
        }

        [Then("the graph should be added to the TripleStore")]
        public void ThenTheGraphShouldBeInTripleStore()
        {
            isthesecondadded.Should().Be(true);
        }
    }
}
