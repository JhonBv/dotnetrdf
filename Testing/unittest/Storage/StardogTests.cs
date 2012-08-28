/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;
using VDS.RDF.Storage.Management;
using VDS.RDF.Storage.Management.Provisioning;
using VDS.RDF.Update;
using VDS.RDF.Writing.Formatting;
using VDS.RDF.Test.Update;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class StardogTests
        : GenericUpdateProcessorTests
    {
        public static StardogConnector GetConnection()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseStardog))
            {
                Assert.Inconclusive("Test Config marks Stardog as unavailable, test cannot be run");
            }
            return new StardogConnector(TestConfigManager.GetSetting(TestConfigManager.StardogServer), TestConfigManager.GetSetting(TestConfigManager.StardogDatabase), TestConfigManager.GetSetting(TestConfigManager.StardogUser), TestConfigManager.GetSetting(TestConfigManager.StardogPassword));
        }

        public static StardogServer GetServer()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseStardog))
            {
                Assert.Inconclusive("Test Config marks Stardog as unavailable, test cannot be run");
            }
            return new StardogServer(TestConfigManager.GetSetting(TestConfigManager.StardogServer), TestConfigManager.GetSetting(TestConfigManager.StardogUser), TestConfigManager.GetSetting(TestConfigManager.StardogPassword));
        }

        protected override IStorageProvider GetManager()
        {
            return (IStorageProvider)StardogTests.GetConnection();
        }

        [TestMethod]
        public void StorageStardogLoadDefaultGraph()
        {
            StardogConnector stardog = StardogTests.GetConnection();;
            Graph g = new Graph();
            stardog.LoadGraph(g, (Uri)null);

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.IsFalse(g.IsEmpty);
        }

        [TestMethod]
        public void StorageStardogLoadNamedGraph()
        {
            StardogConnector stardog = StardogTests.GetConnection();;
            Graph g = new Graph();
            stardog.LoadGraph(g, new Uri("http://example.org/graph"));

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.IsFalse(g.IsEmpty);
        }

        [TestMethod]
        public void StorageStardogSaveToDefaultGraph()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();;
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = null;
                stardog.SaveGraph(g);

                Graph h = new Graph();
                stardog.LoadGraph(h, (Uri)null);
                Console.WriteLine("Retrieved " + h.Triples.Count + " Triple(s) from Stardog");

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
                }
                else
                {
                    Assert.IsTrue(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [TestMethod]
        public void StorageStardogSaveToNamedGraph()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();;
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/graph");
                stardog.SaveGraph(g);

                Graph h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/graph"));

                Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [TestMethod]
        public void StorageStardogSaveToNamedGraph2()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();;
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                Uri u = new Uri("http://example.org/graph/" + DateTime.Now.Ticks);
                g.BaseUri = u;
                stardog.SaveGraph(g);

                Graph h = new Graph();
                stardog.LoadGraph(h, u);

                Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [TestMethod]
        public void StorageStardogSaveToNamedGraphOverwrite()
        {
            try
            {
                //Options.UseBomForUtf8 = false;
                Options.HttpDebugging = true;

                StardogConnector stardog = StardogTests.GetConnection();;
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/namedGraph");
                stardog.SaveGraph(g);

                Graph h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/namedGraph"));

                Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");

                Graph i = new Graph();
                i.LoadFromEmbeddedResource("VDS.RDF.Query.Expressions.LeviathanFunctionLibrary.ttl");
                i.BaseUri = new Uri("http://example.org/namedGraph");
                stardog.SaveGraph(i);

                Graph j = new Graph();
                stardog.LoadGraph(j, "http://example.org/namedGraph");

                Assert.AreNotEqual(g, j, "Retrieved Graph should not be equal to overwritten Graph");
                Assert.AreEqual(i, j, "Retrieved Graph should be equal to Saved Graph");
            }
            finally
            {
                Options.HttpDebugging = false;
                //Options.UseBomForUtf8 = true;
            }
        }

        [TestMethod]
        public void StorageStardogUpdateNamedGraphRemoveTriples()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();;
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/graph");
                stardog.SaveGraph(g);

                INode rdfType = g.CreateUriNode(new Uri(VDS.RDF.Parsing.RdfSpecsHelper.RdfType));

                stardog.UpdateGraph(g.BaseUri, null, g.GetTriplesWithPredicate(rdfType));
                g.Retract(g.GetTriplesWithPredicate(rdfType));

                Graph h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/graph"));

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
                }
                else
                {
                    Assert.IsTrue(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }
                Assert.IsFalse(h.GetTriplesWithPredicate(rdfType).Any(), "Retrieved Graph should not contain any rdf:type Triples");
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [TestMethod]
        public void StorageStardogUpdateNamedGraphAddTriples()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();;
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/addGraph");

                INode rdfType = g.CreateUriNode(new Uri(VDS.RDF.Parsing.RdfSpecsHelper.RdfType));
                Graph types = new Graph();
                types.Assert(g.GetTriplesWithPredicate(rdfType));
                g.Retract(g.GetTriplesWithPredicate(rdfType));

                //Save the Graph without the rdf:type triples
                stardog.SaveGraph(g);
                //Then add back in the rdf:type triples
                stardog.UpdateGraph(g.BaseUri, types.Triples, null);

                Graph h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/addGraph"));

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
                }
                else
                {
                    Assert.IsTrue(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }
                Assert.IsTrue(h.GetTriplesWithPredicate(rdfType).Any(), "Retrieved Graph should not contain any rdf:type Triples");
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [TestMethod]
        public void StorageStardogDeleteNamedGraph()
        {
            try
            {
                //Options.UseBomForUtf8 = false;

                StardogConnector stardog = StardogTests.GetConnection();;
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.BaseUri = new Uri("http://example.org/tempGraph");
                stardog.SaveGraph(g);

                Graph h = new Graph();
                stardog.LoadGraph(h, new Uri("http://example.org/tempGraph"));

                if (g.Triples.Count == h.Triples.Count)
                {
                    Assert.AreEqual(g, h, "Retrieved Graph should be equal to the Saved Graph");
                }
                else
                {
                    Assert.IsTrue(h.HasSubGraph(g), "Retrieved Graph should have the Saved Graph as a subgraph");
                }

                stardog.DeleteGraph("http://example.org/tempGraph");
                Graph i = new Graph();
                stardog.LoadGraph(i, new Uri("http://example.org/tempGraph"));

                Assert.IsTrue(i.IsEmpty, "Retrieved Graph should be empty since it has been deleted");
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
            }
        }

        [TestMethod]
        public void StorageStardogReasoningQL()
        {
            try
            {
                //Options.UseBomForUtf8 = false;
                Options.HttpDebugging = true;

                StardogConnector stardog = StardogTests.GetConnection();;

                Graph g = new Graph();
                g.LoadFromFile("InferenceTest.ttl");
                g.BaseUri = new Uri("http://example.org/reasoning");
                stardog.SaveGraph(g);

                String query = "PREFIX rdfs: <" + NamespaceMapper.RDFS + "> SELECT * WHERE { { ?class rdfs:subClassOf <http://example.org/vehicles/Vehicle> } UNION { GRAPH <http://example.org/reasoning> { ?class rdfs:subClassOf <http://example.org/vehicles/Vehicle> } } }";
                Console.WriteLine(query);
                Console.WriteLine();

                SparqlResultSet resultsNoReasoning = stardog.Query(query) as SparqlResultSet;
                if (resultsNoReasoning != null)
                {
                    Console.WriteLine("Results without Reasoning");
                    TestTools.ShowResults(resultsNoReasoning);
                }
                else
                {
                    Assert.Fail("Did not get a SPARQL Result Set as expected");
                }

                stardog.Reasoning = StardogReasoningMode.QL;
                SparqlResultSet resultsWithReasoning = stardog.Query(query) as SparqlResultSet;
                if (resultsWithReasoning != null)
                {
                    Console.WriteLine("Results with Reasoning");
                    TestTools.ShowResults(resultsWithReasoning);
                }
                else
                {
                    Assert.Fail("Did not get a SPARQL Result Set as expected");
                }

                Assert.IsTrue(resultsWithReasoning.Count >= resultsNoReasoning.Count, "Reasoning should yield as many if not more results");
            }
            finally
            {
                //Options.UseBomForUtf8 = true;
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageStardogTransactionTest()
        {
            try
            {
                Options.HttpDebugging = true;

                StardogConnector stardog = StardogTests.GetConnection();;
                stardog.Begin();
                stardog.Commit();
                stardog.Dispose();
            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageStardogAmpersandsInDataTest()
        {
            try
            {
                Options.HttpDebugging = true;

                StardogConnector stardog = StardogTests.GetConnection();;

                //Save the Graph
                Graph g = new Graph();
                String fragment = "@prefix : <http://example.org/> . [] :string \"This has & ampersands in it\" .";
                g.LoadFromString(fragment);
                g.BaseUri = new Uri("http://example.org/ampersandGraph");

                Console.WriteLine("Original Graph:");
                TestTools.ShowGraph(g);

                stardog.SaveGraph(g);

                //Retrieve and check it round trips
                Graph h = new Graph();
                stardog.LoadGraph(h, g.BaseUri);

                Console.WriteLine("Graph as retrieved from Stardog:");
                TestTools.ShowGraph(h);

                Assert.AreEqual(g, h, "Graphs should be equal");
                
                //Now try to delete the data from this Graph
                GenericUpdateProcessor processor = new GenericUpdateProcessor(stardog);
                SparqlUpdateParser parser = new SparqlUpdateParser();
                processor.ProcessCommandSet(parser.ParseFromString("WITH <http://example.org/ampersandGraph> DELETE WHERE { ?s ?p ?o }"));

                Graph i = new Graph();
                stardog.LoadGraph(i, g.BaseUri);

                Console.WriteLine("Graph as retrieved after the DELETE WHERE:");
                TestTools.ShowGraph(i);

                Assert.AreNotEqual(g, i, "Graphs should not be equal");
                Assert.AreNotEqual(h, i, "Graphs should not be equal");

            }
            finally
            {
                Options.HttpDebugging = false;
            }
        }

        [TestMethod]
        public void StorageStardogCreateNewStore()
        {
            Guid guid;
            do
            {
                guid = Guid.NewGuid();
            } while (guid.Equals(Guid.Empty) || !Char.IsLetter(guid.ToString()[0]));

            StardogServer stardog = StardogTests.GetServer();
            IStoreTemplate template = stardog.GetDefaultTemplate(guid.ToString());
            Console.WriteLine("Template ID " + template.ID);

            try
            {
                Options.HttpDebugging = true;
                //Options.HttpFullDebugging = true;

                stardog.CreateStore(template);
            }
            finally
            {
                //Options.HttpFullDebugging = false;
                Options.HttpDebugging = false;
            }

            stardog.Dispose();
        }
    }
}