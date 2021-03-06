/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.Arq
{
    /// <summary>
    /// Represents the ARQ afn:now() function.
    /// </summary>
    public class NowFunction 
        : ISparqlExpression
    {
        private SparqlQuery _currQuery;
        private IValuedNode _node;

        private readonly object lockObject = new object();

        /// <summary>
        /// Gets the value of the function in the given Evaluation Context for the given Binding ID.
        /// </summary>
        /// <param name="context">Evaluation Context.</param>
        /// <param name="bindingID">Binding ID.</param>
        /// <returns>
        /// Returns a constant Literal Node which is a Date Time typed Literal.
        /// </returns>
        public IValuedNode Evaluate(SparqlEvaluationContext context, int bindingID)
        {
            if (_currQuery == null)
            {
                _currQuery = context.Query;
            }
            if (_node == null || !ReferenceEquals(_currQuery, context.Query))
            {
                lock(lockObject)
                {
                    if (_node == null || !ReferenceEquals(_currQuery, context.Query))
                    {
                        _node = new DateTimeNode(null, DateTime.Now);
                    }
                }
            }
            return _node;
        }

        /// <summary>
        /// Gets the Type of the Expression.
        /// </summary>
        public SparqlExpressionType Type
        {
            get
            {
                return SparqlExpressionType.Function;
            }
        }

        /// <summary>
        /// Gets the Functor of the Expression.
        /// </summary>
        public virtual string Functor
        {
            get
            {
                return ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Now;
            }
        }

        /// <summary>
        /// Gets whether an expression can safely be evaluated in parallel.
        /// </summary>
        public virtual bool CanParallelise
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the String representation of the function.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + ArqFunctionFactory.ArqFunctionsNamespace + ArqFunctionFactory.Now + ">()";
        }

        /// <summary>
        /// Gets the variables in the expression.
        /// </summary>
        public IEnumerable<string> Variables
        {
            get 
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Gets the arguments of the expression.
        /// </summary>
        public IEnumerable<ISparqlExpression> Arguments
        {
            get 
            {
                return Enumerable.Empty<ISparqlExpression>(); 
            }
        }

        /// <summary>
        /// Returns the expression as there are no arguments to be transformed.
        /// </summary>
        /// <param name="transformer">Expression Transformer.</param>
        /// <returns></returns>
        public ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return this;
        }
    }
}
