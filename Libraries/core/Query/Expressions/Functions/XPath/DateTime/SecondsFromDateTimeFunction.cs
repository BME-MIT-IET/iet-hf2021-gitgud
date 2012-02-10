﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Expressions.Functions.XPath.DateTime
{
    /// <summary>
    /// Represents the XPath seconds-from-dateTime() function
    /// </summary>
    public class SecondsFromDateTimeFunction
        : BaseUnaryDateTimeFunction
    {
        /// <summary>
        /// Creates a new XPath Seconds from Date Time function
        /// </summary>
        /// <param name="expr">Expression</param>
        public SecondsFromDateTimeFunction(ISparqlExpression expr)
            : base(expr) { }

        /// <summary>
        /// Calculates the numeric value of the function from the given Date Time
        /// </summary>
        /// <param name="dateTime">Date Time</param>
        /// <returns></returns>
        protected override IValuedNode ValueInternal(DateTimeOffset dateTime)
        {
            decimal seconds = Convert.ToDecimal(dateTime.Second);
            seconds += ((decimal)dateTime.Millisecond) / 1000m;

            return new DecimalNode(null, seconds);
        }

        /// <summary>
        /// Gets the String representation of the function
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.SecondsFromDateTime + ">(" + this._expr.ToString() + ")";
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.SecondsFromDateTime;
            }
        }

        /// <summary>
        /// Transforms the Expression using the given Transformer
        /// </summary>
        /// <param name="transformer">Expression Transformer</param>
        /// <returns></returns>
        public override ISparqlExpression Transform(IExpressionTransformer transformer)
        {
            return new SecondsFromDateTimeFunction(transformer.Transform(this._expr));
        }
    }
}