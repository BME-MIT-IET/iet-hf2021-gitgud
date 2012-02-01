﻿/*

Copyright Robert Vesse 2009-12
rvesse@vdesign-studios.com

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.Common;

namespace VDS.RDF.Query.Datasets
{
    /// <summary>
    /// Abstract Base class of dataset designed around out of memory datasets where you rarely wish to load data into memory but simply wish to know which graph to look in for data
    /// </summary>
    public abstract class BaseQuadDataset
        : ISparqlDataset
    {
        private readonly ThreadIsolatedReference<Stack<IEnumerable<Uri>>> _defaultGraphs;
        private readonly ThreadIsolatedReference<Stack<IEnumerable<Uri>>> _activeGraphs;
        private bool _unionDefaultGraph = true;
        private Uri _defaultGraphUri;

        public BaseQuadDataset()
        {
            this._defaultGraphs = new ThreadIsolatedReference<Stack<IEnumerable<Uri>>>(this.InitDefaultGraphStack);
            this._activeGraphs = new ThreadIsolatedReference<Stack<IEnumerable<Uri>>>(this.InitActiveGraphStack);
        }

        public BaseQuadDataset(bool unionDefaultGraph)
            : this()
        {
            this._unionDefaultGraph = unionDefaultGraph;
        }

        public BaseQuadDataset(Uri defaultGraphUri)
            : this(false)
        {
            this._defaultGraphUri = defaultGraphUri;
        }

        private Stack<IEnumerable<Uri>> InitDefaultGraphStack()
        {
            Stack<IEnumerable<Uri>> s = new Stack<IEnumerable<Uri>>();
            if (!this._unionDefaultGraph)
            {
                s.Push(new Uri[] { this._defaultGraphUri });
            }
            return s;
        }

        private Stack<IEnumerable<Uri>> InitActiveGraphStack()
        {
            return new Stack<IEnumerable<Uri>>();
        }

        #region Active and Default Graph management

        public void SetActiveGraph(IEnumerable<Uri> graphUris)
        {
            this._activeGraphs.Value.Push(graphUris.ToList());
        }

        public void SetActiveGraph(Uri graphUri)
        {
            if (graphUri == null)
            {
                this._activeGraphs.Value.Push(this.DefaultGraphUris);
            }
            else
            {
                this._activeGraphs.Value.Push(new Uri[] { graphUri });
            }
        }

        public void SetDefaultGraph(Uri graphUri)
        {
            this._defaultGraphs.Value.Push(new Uri[] { graphUri });
        }

        public void SetDefaultGraph(IEnumerable<Uri> graphUris)
        {
            this._defaultGraphs.Value.Push(graphUris.ToList());
        }

        public void ResetActiveGraph()
        {
            if (this._activeGraphs.Value.Count > 0)
            {
                this._activeGraphs.Value.Pop();
            }
            else
            {
                throw new RdfQueryException("Unable to reset the Active Graph since no previous Active Graphs exist");
            }
        }

        public void ResetDefaultGraph()
        {
            if (this._defaultGraphs.Value.Count > 0)
            {
                this._defaultGraphs.Value.Pop();
            }
            else
            {
                throw new RdfQueryException("Unable to reset the Default Graph since no previous Default Graphs exist");
            }
        }

        public IEnumerable<Uri> DefaultGraphUris
        {
            get 
            {
                if (this._defaultGraphs.Value.Count > 0)
                {
                    return this._defaultGraphs.Value.Peek();
                }
                else if (this._unionDefaultGraph)
                {
                    return this.GraphUris;
                }
                else
                {
                    return Enumerable.Empty<Uri>();
                }
            }
        }

        public IEnumerable<Uri> ActiveGraphUris
        {
            get 
            {
                if (this._activeGraphs.Value.Count > 0)
                {
                    return this._activeGraphs.Value.Peek();
                }
                else
                {
                    return this.DefaultGraphUris;
                }
            }
        }

        public bool UsesUnionDefaultGraph
        {
            get 
            {
                return this._unionDefaultGraph;
            }
        }

        /// <summary>
        /// Gets whether the given URI represents the default graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected bool IsDefaultGraph(Uri graphUri)
        {
            return EqualityHelper.AreUrisEqual(graphUri, this._defaultGraphUri);
        }

        #endregion

        public virtual void AddGraph(IGraph g)
        {
            foreach (Triple t in g.Triples)
            {
                this.AddQuad(g.BaseUri, t);
            }
        }

        protected internal abstract void AddQuad(Uri graphUri, Triple t);

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public abstract void RemoveGraph(Uri graphUri);

        protected internal abstract void RemoveQuad(Uri graphUri, Triple t);

        /// <summary>
        /// Gets whether a Graph with the given URI is the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public bool HasGraph(Uri graphUri)
        {
            if (graphUri == null || graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri))
            {
                if (this.DefaultGraphUris.Any() != null)
                {
                    return true;
                }
                else
                {
                    return this.HasGraphInternal(null);
                }
            }
            else
            {
                return this.HasGraphInternal(graphUri);
            }
        }

        /// <summary>
        /// Determines whether a given Graph exists in the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected abstract bool HasGraphInternal(Uri graphUri);

        public virtual IEnumerable<IGraph> Graphs
        {
            get 
            {
                return (from u in this.GraphUris
                        select this[u]);
            }
        }

        public abstract IEnumerable<Uri> GraphUris
        {
            get;
        }

        /// <summary>
        /// Gets the Graph with the given URI from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This property need only return a read-only view of the Graph, code which wishes to modify Graphs should use the <see cref="ISparqlDataset.GetModifiableGraph">GetModifiableGraph()</see> method to guarantee a Graph they can modify and will be persisted to the underlying storage
        /// </para>
        /// </remarks>
        public virtual IGraph this[Uri graphUri]
        {
            get
            {
                if (graphUri == null || graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri))
                {
                    if (this.DefaultGraphUris.Any())
                    {
                        if (this.DefaultGraphUris.Count() == 1)
                        {
                            return new Graph(new QuadDatasetTripleCollection(this, this.DefaultGraphUris.First()));
                        }
                        else
                        {
                            IEnumerable<IGraph> gs = (from u in this.DefaultGraphUris
                                                      select new Graph(new QuadDatasetTripleCollection(this, u))).OfType<IGraph>();
                            return new UnionGraph(gs.First(), gs.Skip(1));
                        }
                    }
                    else
                    {
                        return this.GetGraphInternal(null);
                    }
                }
                else
                {
                    return this.GetGraphInternal(graphUri);
                }
            }
        }

        protected abstract IGraph GetGraphInternal(Uri graphUri);

        public virtual IGraph GetModifiableGraph(Uri graphUri)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        public virtual bool HasTriples
        {
            get 
            {
                return this.Triples.Any();
            }
        }

        public bool ContainsTriple(Triple t)
        {
            return this.ActiveGraphUris.Any(u => this.ContainsQuad(u, t));
        }

        /// <summary>
        /// Gets whether a Triple exists in a specific Graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        protected abstract internal bool ContainsQuad(Uri graphUri, Triple t);

        public IEnumerable<Triple> Triples
        {
            get 
            { 
                return (from u in this.ActiveGraphUris
                        from t in this.GetQuads(u)
                        select t);
            }
        }

        /// <summary>
        /// Gets all the Triples for a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected abstract internal IEnumerable<Triple> GetQuads(Uri graphUri);

        public IEnumerable<Triple> GetTriplesWithSubject(INode subj)
        {
            return (from u in this.ActiveGraphUris
                    from t in this.GetQuadsWithSubject(u, subj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given subject from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="subj">Subject</param>
        /// <returns></returns>
        protected abstract internal IEnumerable<Triple> GetQuadsWithSubject(Uri graphUri, INode subj);

        public IEnumerable<Triple> GetTriplesWithPredicate(INode pred)
        {
            return (from u in this.ActiveGraphUris
                    from t in this.GetQuadsWithPredicate(u, pred)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given predicate from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected abstract internal IEnumerable<Triple> GetQuadsWithPredicate(Uri graphUri, INode pred);

        public IEnumerable<Triple> GetTriplesWithObject(INode obj)
        {
            return (from u in this.ActiveGraphUris
                    from t in this.GetQuadsWithObject(u, obj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given object from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected abstract internal IEnumerable<Triple> GetQuadsWithObject(Uri graphUri, INode obj);

        public IEnumerable<Triple> GetTriplesWithSubjectPredicate(INode subj, INode pred)
        {
            return (from u in this.ActiveGraphUris
                    from t in this.GetQuadsWithSubjectPredicate(u, subj, pred)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given subject and predicate from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="subj">Subject</param>
        /// <param name="pred">Predicate</param>
        /// <returns></returns>
        protected abstract internal IEnumerable<Triple> GetQuadsWithSubjectPredicate(Uri graphUri, INode subj, INode pred);

        public IEnumerable<Triple> GetTriplesWithSubjectObject(INode subj, INode obj)
        {
            return (from u in this.ActiveGraphUris
                    from t in this.GetQuadsWithSubjectObject(u, subj, obj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given subject and object from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="subj">Subject</param>
        /// <param name="obj">Object</param>
        /// <returns></returns>
        protected abstract internal IEnumerable<Triple> GetQuadsWithSubjectObject(Uri graphUri, INode subj, INode obj);

        public IEnumerable<Triple> GetTriplesWithPredicateObject(INode pred, INode obj)
        {
            return (from u in this.ActiveGraphUris
                    from t in this.GetQuadsWithPredicateObject(u, pred, obj)
                    select t);
        }

        /// <summary>
        /// Gets all the Triples with a given predicate and object from a specific graph of the dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <param name="obj">Predicate</param>
        /// <param name="subj">Object</param>
        /// <returns></returns>
        protected abstract internal IEnumerable<Triple> GetQuadsWithPredicateObject(Uri graphUri, INode pred, INode obj);

        public virtual void Flush()
        {
            //Nothing to do
        }

        public virtual void Discard()
        {
            //Nothing to do
        }
    }

    /// <summary>
    /// Abstract Base class for immutable quad datasets
    /// </summary>
    public abstract class BaseImmutableQuadDataset
        : BaseQuadDataset
    {
        public sealed override void AddGraph(IGraph g)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        protected internal override void AddQuad(Uri graphUri, Triple t)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        public sealed override void RemoveGraph(Uri graphUri)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        protected internal override void RemoveQuad(Uri graphUri, Triple t)
        {
            throw new NotSupportedException("This dataset is immutable");
        }

        public sealed override IGraph GetModifiableGraph(Uri graphUri)
        {
            throw new NotSupportedException("This dataset is immutable");
        }
    }

    /// <summary>
    /// Abstract Base class for quad datasets that support transactions
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Transaction implementation of dotNetRDF is based upon a MRSW concurrency model, since only one writer may be active changes are immediately pushed to the dataset and visible within the transaction and they are committed or rolled back when <see cref="BaseTransactionalDataset.Flush()">Flush()</see> or <see cref="BaseTransactionalDataset.Discard()">Discard()</see> are called.
    /// </para>
    /// <para>
    /// So in practical terms it is perfectly OK for the storage to be updated during a transaction because if the transaction fails the changes will be rolled back because all changes are stored in-memory until the end of the transaction.  This may not be an ideal transaction model for all scenarios so you may wish to implement your own version of transactions or code your implementations of the abstract methods accordingly to limit actual persistence to the end of a transaction.
    /// </para>
    /// </remarks>
    public abstract class BaseTransactionalQuadDataset
        : BaseQuadDataset
    {
        private List<GraphPersistenceAction> _actions = new List<GraphPersistenceAction>();
        private TripleStore _modifiableGraphs = new TripleStore();

        public BaseTransactionalQuadDataset()
            : base() { }

        public BaseTransactionalQuadDataset(bool unionDefaultGraph)
            : base(unionDefaultGraph) { }

        public BaseTransactionalQuadDataset(Uri defaultGraphUri)
            : base(defaultGraphUri) { }

        /// <summary>
        /// Adds a Graph to the Dataset
        /// </summary>
        /// <param name="g">Graph to add</param>
        public sealed override void AddGraph(IGraph g)
        {
            if (this.HasGraph(g.BaseUri))
            {
                ITransactionalGraph existing = (ITransactionalGraph)this.GetModifiableGraph(g.BaseUri);
                this._actions.Add(new GraphPersistenceAction(existing, GraphPersistenceActionType.Modified));
            }
            else
            {
                this._actions.Add(new GraphPersistenceAction(g, GraphPersistenceActionType.Added));
            }
            this.AddGraphInternal(g);
        }

        /// <summary>
        /// Adds a Graph to the Dataset
        /// </summary>
        /// <param name="g">Graph to add</param>
        protected abstract void AddGraphInternal(IGraph g);

        /// <summary>
        /// Removes a Graph from the Dataset
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        public sealed override void RemoveGraph(Uri graphUri)
        {
            if (graphUri == null || graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri))
            {
                if (this.DefaultGraphUris.Any())
                {
                    foreach (Uri u in this.DefaultGraphUris)
                    {
                        if (this.IsDefaultGraph(u))
                        {
                            //Default Graph gets cleared
                            GraphPersistenceWrapper wrapper = new GraphPersistenceWrapper(this[u]);
                            wrapper.Clear();
                            this._actions.Add(new GraphPersistenceAction(wrapper, GraphPersistenceActionType.Modified));
                        }
                        else
                        {
                            //Other Graphs get actually deleted
                            this._actions.Add(new GraphPersistenceAction(this[u], GraphPersistenceActionType.Deleted));
                        }
                    }
                }
                else if (this.HasGraph(graphUri))
                {
                    this._actions.Add(new GraphPersistenceAction(this[graphUri], GraphPersistenceActionType.Deleted));
                    this.RemoveGraphInternal(graphUri);
                }
            }
            else if (this.HasGraph(graphUri))
            {
                this._actions.Add(new GraphPersistenceAction(this[graphUri], GraphPersistenceActionType.Deleted));
                this.RemoveGraphInternal(graphUri);
            }
        }

        protected abstract void RemoveGraphInternal(Uri graphUri);

        public override IGraph this[Uri graphUri]
        {
            get
            {
                if (graphUri == null || graphUri.ToSafeString().Equals(GraphCollection.DefaultGraphUri))
                {
                    if (this.DefaultGraphUris.Any())
                    {
                        if (this.DefaultGraphUris.Count() == 1)
                        {
                            return new Graph(new QuadDatasetTripleCollection(this, this.DefaultGraphUris.First()));
                        }
                        else
                        {
                            IEnumerable<IGraph> gs = (from u in this.DefaultGraphUris
                                                      select new Graph(new QuadDatasetTripleCollection(this, u))).OfType<IGraph>();
                            return new UnionGraph(gs.First(), gs.Skip(1));
                        }
                    }
                    else if (this._modifiableGraphs.HasGraph(graphUri))
                    {
                        return this._modifiableGraphs.Graph(graphUri);
                    }
                    else
                    {
                        return this.GetGraphInternal(null);
                    }
                }
                else if (this._modifiableGraphs.HasGraph(graphUri))
                {
                    return this._modifiableGraphs.Graph(graphUri);
                }
                else
                {
                    return this.GetGraphInternal(graphUri);
                }
            }
        }

        /// <summary>
        /// Gets a Graph from the Dataset that can be modified
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        public sealed override IGraph GetModifiableGraph(Uri graphUri)
        {
            if (!this._modifiableGraphs.HasGraph(graphUri))
            {
                IGraph current = this.GetModifiableGraphInternal(graphUri);
                if (!this._modifiableGraphs.HasGraph(current.BaseUri))
                {
                    this._modifiableGraphs.Add(current);
                }
                graphUri = current.BaseUri;
            }
            ITransactionalGraph existing = (ITransactionalGraph)this._modifiableGraphs.Graph(graphUri);
            this._actions.Add(new GraphPersistenceAction(existing, GraphPersistenceActionType.Modified));
            return existing;
        }

        /// <summary>
        /// Gets a Graph from the Dataset that can be modified transactionally
        /// </summary>
        /// <param name="graphUri">Graph URI</param>
        /// <returns></returns>
        protected abstract ITransactionalGraph GetModifiableGraphInternal(Uri graphUri);

        /// <summary>
        /// Ensures that any changes to the Dataset (if any) are flushed to the underlying Storage
        /// </summary>
        /// <remarks>
        /// Commits the Active Transaction
        /// </remarks>
        public sealed override void Flush()
        {
            int i = 0;
            while (i < this._actions.Count)
            {
                GraphPersistenceAction action = this._actions[i];
                switch (action.Action)
                {
                    case GraphPersistenceActionType.Added:
                        //If Graph was added ensure any changes were flushed
                        action.Graph.Flush();
                        break;
                    case GraphPersistenceActionType.Deleted:
                        //If Graph was deleted can discard any changes
                        action.Graph.Discard();
                        break;
                    case GraphPersistenceActionType.Modified:
                        //If Graph was modified ensure any changes were flushed
                        action.Graph.Flush();
                        break;
                }
                i++;
            }
            this._actions.Clear();
            //Ensure any Modifiable Graphs we've looked at have been Flushed()
            foreach (ITransactionalGraph g in this._modifiableGraphs.Graphs.OfType<ITransactionalGraph>())
            {
                g.Flush();
            }
            this._modifiableGraphs = new TripleStore();

            this.FlushInternal();
        }

        /// <summary>
        /// Ensures that any changes to the Dataset (if any) are discarded
        /// </summary>
        /// <remarks>
        /// Rollsback the Active Transaction
        /// </remarks>
        public sealed override void Discard()
        {
            int i = this._actions.Count - 1;
            int total = this._actions.Count;
            while (i >= 0)
            {
                GraphPersistenceAction action = this._actions[i];
                switch (action.Action)
                {
                    case GraphPersistenceActionType.Added:
                        //If a Graph was added we must now remove it
                        if (this.HasGraphInternal(action.Graph.BaseUri))
                        {
                            this.RemoveGraphInternal(action.Graph.BaseUri);
                        }
                        break;
                    case GraphPersistenceActionType.Deleted:
                        //If a Graph was deleted we must now add it back again
                        //Don't add the full Graph only an empty Graph with the given URI
                        Graph g = new Graph();
                        g.BaseUri = action.Graph.BaseUri;
                        this.AddGraphInternal(g);
                        break;
                    case GraphPersistenceActionType.Modified:
                        //If a Graph was modified we must discard the changes
                        action.Graph.Discard();
                        break;
                }
                i--;
            }
            if (total == this._actions.Count)
            {
                this._actions.Clear();
            }
            else
            {
                this._actions.RemoveRange(0, total);
            }
            //Ensure any modifiable Graphs we've looked at have been Discarded
            foreach (ITransactionalGraph g in this._modifiableGraphs.Graphs.OfType<ITransactionalGraph>())
            {
                g.Discard();
            }
            this._modifiableGraphs = new TripleStore();

            this.DiscardInternal();
        }

        /// <summary>
        /// Allows the derived dataset to take any post-Flush() actions required
        /// </summary>
        protected virtual void FlushInternal()
        {
            //No actions by default
        }

        /// <summary>
        /// Allows the derived dataset to take any post-Discard() actions required
        /// </summary>
        protected virtual void DiscardInternal()
        {
            //No actions by default
        }
    }
}
