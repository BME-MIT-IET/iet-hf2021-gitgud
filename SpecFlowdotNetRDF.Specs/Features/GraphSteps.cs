using System;
using TechTalk.SpecFlow;
using VDS.RDF;
using Xunit;

namespace SpecFlowdotNetRDF.Specs.Features
{
    [Binding]
    public class GraphSteps
    {
        private Graph firstgraph = new Graph();
        private Graph secondgraph = new Graph();
        GraphDiffReport graphreport = new GraphDiffReport();

        [Given(@"There is a graph")]
        public void GivenThereIsAGraph()
        {
            firstgraph.LoadFromFile("resources\\Turtle.ttl");
        }
        
        [Given(@"The second graph is different")]
        public void GivenTheSecondGraphIsDifferent()
        {
            secondgraph.LoadFromFile("resources\\InferenceTest.ttl");
        }
        
        [Given(@"The second graph is the same")]
        public void GivenTheSecondGraphIsTheSame()
        {
            secondgraph.LoadFromFile("resources\\Turtle.ttl");
        }
        
        [When(@"The two graphs are compared, and the result is put into a report")]
        public void WhenTheTwoGraphsAreComparedAndTheResultIsPutIntoAReport()
        {
            this.graphreport = firstgraph.Difference(secondgraph);
        }
        
        [Then(@"The report should say that Graphs are not equal")]
        public void ThenTheReportShouldSayThatGraphsAreNotEqual()
        {
            Assert.False(graphreport.AreEqual);
        }
        
        [Then(@"The report should say that Graphs are equal")]
        public void ThenTheReportShouldSayThatGraphsAreEqual()
        {
            Assert.True(graphreport.AreEqual);
        }
    }
}
