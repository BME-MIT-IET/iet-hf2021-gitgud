using System;
using TechTalk.SpecFlow;
using VDS.RDF;
using VDS.RDF.Parsing;
using Xunit;

namespace SpecFlowdotNetRDF.Specs.Features
{
    [Binding]
    class SpecflowGraphSteps
    {
        private IGraph g = new Graph();
        private Uri expected;
        private IUriNode actual;

        private Graph h = new Graph();
        private Graph j;

        [Given(@"a graph with a base URI")]
        public void GivenAGraphWithBaseURI()
        {
            g.BaseUri = new Uri("http://example.org/");
        }

        [Given(@"an URI with a path")]
        public void GivenAnURI()
        {
            expected = new Uri("http://example.org/relative/path");
        }

        [Given(@"a graph with 1 triple")]
        public void GivenAGraphWith1Triple()
        {
            h.Assert(h.CreateBlankNode(), h.CreateBlankNode(), h.CreateBlankNode());
        }

        [Given(@"an other empty graph")]
        public void GivenAnotherEmptyGraph()
        {
            j = new Graph();
        }

        [When(@"creating a URI node for the graph with the same path as the other URI")]
        public void CreatingURINodeForTheGraph()
        {
            actual = g.CreateUriNode(new Uri("relative/path", UriKind.Relative));
        }

        [When(@"merging the two graphs")]
        public void MergingTwoGraphs()
        {
            h.Merge(j);
        }

        [Then(@"the URI node of the graph and the and the URI should be equal")]
        public void TheURINodeOfGraphAndTheURIShouldBeEqual()
        {
            Assert.Equal(expected, actual.Uri);
        }

        [Then(@"the resulting graph should have 1 triple")]
        public void ResultingGraphShouldHave1Triple()
        {
            Assert.Equal(1, h.Triples.Count);
        }
     
    }
}
