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
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class ParallelEvaluation
    {
        private InMemoryDataset _dataset;
        private SparqlQueryParser _parser = new SparqlQueryParser();
        private SparqlFormatter _formatter = new SparqlFormatter();
        private LeviathanQueryProcessor _processor;
        private const int TripleLimit = 150;

        private void EnsureTestData()
        {
            if (this._dataset == null)
            {
                this._dataset = new InMemoryDataset();
                Graph g = new Graph();
                g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
                g.Retract(g.Triples.Where(t => !t.IsGroundTriple).ToList());
                if (g.Triples.Count > TripleLimit) g.Retract(g.Triples.Skip(TripleLimit).ToList());
                this._dataset.AddGraph(g);

                this._processor = new LeviathanQueryProcessor(this._dataset);
            }
        }

        private void TestQuery(String query)
        {
            this.EnsureTestData();

            SparqlQuery q = this._parser.ParseFromString(query);

            Console.WriteLine("Query:");
            Console.WriteLine(this._formatter.Format(q));
            Console.WriteLine();

            Console.WriteLine("Normal Algebra:");
            Console.WriteLine(q.ToAlgebra().ToString());
            Console.WriteLine();

            Stopwatch timer = new Stopwatch();

            //Evaluate normally
            timer.Start();
            Object normResults = this._processor.ProcessQuery(q);
            timer.Stop();
            Console.WriteLine("Normal Evaluation took " + timer.Elapsed);
            timer.Reset();

            if (normResults is SparqlResultSet)
            {
                SparqlResultSet rsetNorm = (SparqlResultSet)normResults;
                Console.WriteLine("Normal Evaluation returned " + rsetNorm.Count + " Result(s)");
                Console.WriteLine();

                //Evaluate parallelised
                q.AlgebraOptimisers = new IAlgebraOptimiser[] { new ParallelEvaluationOptimiser() };
                Console.WriteLine("Parallel Algebra:");
                Console.WriteLine(q.ToAlgebra().ToString());
                Console.WriteLine();

                timer.Start();
                Object parResults = this._processor.ProcessQuery(q);
                timer.Stop();
                Console.WriteLine("Parallel Evaluation took " + timer.Elapsed);

                if (parResults is SparqlResultSet)
                {
                    SparqlResultSet rsetPar = (SparqlResultSet)parResults;
                    Console.WriteLine("Parallel Evaluation returned " + rsetPar.Count + " Result(s)");
                    Assert.AreEqual(rsetNorm.Count, rsetPar.Count, "Result Sets should have same number of results");
                    Assert.AreEqual(rsetNorm, rsetPar, "Result Sets should be equal");
                }
                else
                {
                    Assert.Fail("Query did not return a SPARQL Result Set as expected");
                }
            }
            else
            {
                Assert.Fail("Query did not return a SPARQL Result Set for normal evaluation as expected");
            }
        }

        [TestMethod]
        public void SparqlParallelEvaluationJoin1()
        {
            this.TestQuery("SELECT * WHERE { ?s ?p ?o { ?x ?y ?z } }");
        }

        [TestMethod]
        public void SparqlParallelEvaluationJoin2()
        {
            try
            {
                this.TestQuery("SELECT * WHERE { ?s ?p ?o { ?x ?y ?z } { ?a ?b ?c } }");
            }
            catch (OutOfMemoryException outEx)
            {
                TestTools.ReportError("Out of Memory", outEx);
            }
        }
    }
}