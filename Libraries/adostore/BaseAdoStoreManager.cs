﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Writing;

namespace VDS.RDF.Storage
{
    public abstract class BaseAdoStore<TConn,TCommand,TParameter,TAdaptor,TException> : IGenericIOManager, IDisposable
        where TConn : DbConnection
        where TCommand : DbCommand
        where TParameter : DbParameter
        where TAdaptor : DbDataAdapter
        where TException : DbException
    {
        private TConn _connection;

        #region Constructor and Destructor

        public BaseAdoStore(TConn connection)
        {
            this._connection = connection;
            this._connection.Open();

            //Do a Version Check
            this.CheckVersion();
        }

        /// <summary>
        /// Finalizer for the Store Manager which ensures the
        /// </summary>
        ~BaseAdoStore()
        {
            this.Dispose(false);
        }

        #endregion

        #region Abstract Implementation

        /// <summary>
        /// Gets a Command for sending SQL Commands to the underlying Database
        /// </summary>
        /// <returns></returns>
        protected abstract TCommand GetCommand();

        protected abstract TParameter GetParameter(String name);

        /// <summary>
        /// Gets an Adaptor for converting results from SQL queries on the underlying Database into a DataTable
        /// </summary>
        /// <returns></returns>
        protected abstract TAdaptor GetAdaptor();

        /// <summary>
        /// Ensures that the Database is setup and returns the Version of the Database Schema
        /// </summary>
        /// <param name="connection">Database Connection</param>
        /// <returns>The Version of the Database Schema</returns>
        protected abstract int EnsureSetup(TConn connection);

        #endregion

        #region Internal Implementation

        /// <summary>
        /// Checks the Version of the Store
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is intended for two purposes
        /// <ol>
        ///     <li>Future proofing so later versions of the library can add additional stored procedures to the database and the code can decide which are available to it</li>
        ///     <li>Detecting when users try to use the class to connect to legacy databases created with the old Schema which are not compatible with this code</li>
        /// </para>
        /// </remarks>
        public int CheckVersion()
        {
            try
            {
                TCommand cmd = this.GetCommand();
                cmd.CommandText = "GetVersion";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(this.GetParameter("RC"));
                cmd.Parameters["RC"].DbType = DbType.Int32;
                cmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;
                cmd.Connection = this._connection;
                cmd.ExecuteNonQuery();

                int version = (int)cmd.Parameters["RC"].Value;
                switch (version)
                {
                    case 1:
                        //OK
                        return version;
                    default:
                        throw new RdfStorageException("Unknown ADO Store Version");
                }
            }
            catch (TException ex)
            {
                if (ex.Message.IndexOf("permission", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw new RdfStorageException("Unable to connect to an ADO Store as it appears you may not have the necessary permissions on this database, see inner exception for details.  Users for ADO stores should be added to one of the roles rdf_readwrite, rdf_readinsert or rdf_readonly", ex);
                }

                //If we get an SQL Exception then it may mean we've been used to try to connect to a legacy
                //SQL Store so check this now
                try
                {
                    //Try the following SQL, if we are talking to a legacy store it will have the
                    //graphHash field which doesn't appear in the new database schema
                    this.ExecuteScalar("SELECT graphHash FROM GRAPHS WHERE graphID=1");

                    //If it executes succesfully then it's a legacy store
                    //REQ: Add a link to the documentation on upgrading
                    throw new RdfStorageException("The underlying Database appears to be a legacy SQL Store using the old dotNetRDF Store Format.  You may connect to this for the time being using one of the old ISqlIOManager implementations but should see the documentation at ?? with regards to upgrading your store");
                }
                catch (TException)
                {
                    //If this check errors then not a legacy store so may just be not set up yet
                    return this.EnsureSetup(this._connection);
                }
            }
        }

        /// <summary>
        /// Executes a Scalar Query on the Database
        /// </summary>
        /// <param name="query">SQL Query</param>
        /// <returns></returns>
        internal Object ExecuteScalar(String query)
        {
            TCommand cmd = this.GetCommand();
            cmd.CommandText = query;
            cmd.Connection = this._connection;

            return cmd.ExecuteScalar();
        }

        internal DbDataReader GetReader(String query)
        {
            return this.GetReader(query, CommandType.Text);
        }

        internal DbDataReader GetReader(String query, CommandType type)
        {
            TCommand cmd = this.GetCommand();
            cmd.CommandType = type;
            cmd.CommandText = query;
            cmd.Connection = this._connection;

            return cmd.ExecuteReader();
        }

        internal void EncodeNode(TCommand cmd, INode n)
        {
            this.EncodeNode(cmd, n, null);
        }

        internal void EncodeNode(TCommand cmd, INode n, TripleSegment? segment)
        {
            String prefix = "node";
            if (segment != null)
            {
                switch (segment)
                {
                    case TripleSegment.Subject:
                        prefix = "subject";
                        break;
                    case TripleSegment.Predicate:
                        prefix = "predicate";
                        break;
                    case TripleSegment.Object:
                        prefix = "object";
                        break;
                }
            }

            //Node Type Parameter
            if (!cmd.Parameters.Contains(prefix + "Type")) cmd.Parameters.Add(this.GetParameter(prefix + "Type"));
            cmd.Parameters[prefix + "Type"].DbType = DbType.Byte;
            cmd.Parameters[prefix + "Type"].Value = (byte)n.NodeType;

            //Node Value Parameter
            if (!cmd.Parameters.Contains(prefix + "Value")) cmd.Parameters.Add(this.GetParameter(prefix + "Value"));
            cmd.Parameters[prefix + "Value"].DbType = DbType.String;
            switch (n.NodeType)
            {
                case NodeType.Blank:
                    cmd.Parameters[prefix + "Value"].Value = n.ToString();
                    break;
                case NodeType.Literal:
                    cmd.Parameters[prefix + "Value"].Value = ((LiteralNode)n).Value;
                    break;
                case NodeType.Uri:
                    cmd.Parameters[prefix + "Value"].Value = n.ToString();
                    break;
                default:
                    throw new NotSupportedException("Only Blank, URI and Literal Nodes are currently supported");
            }

            //Node Meta Parameter
            if (n.NodeType == NodeType.Literal)
            {
                LiteralNode lit = (LiteralNode)n;
                if (lit.DataType != null)
                {
                    if (!cmd.Parameters.Contains(prefix + "Meta")) cmd.Parameters.Add(this.GetParameter(prefix + "Meta"));
                    cmd.Parameters[prefix + "Meta"].DbType = DbType.String;
                    cmd.Parameters[prefix + "Meta"].Value = lit.DataType.ToString();
                }
                else if (!lit.Language.Equals(String.Empty))
                {
                    if (!cmd.Parameters.Contains(prefix + "Meta")) cmd.Parameters.Add(this.GetParameter(prefix + "Meta"));
                    cmd.Parameters[prefix + "Meta"].DbType = DbType.String;
                    cmd.Parameters[prefix + "Meta"].Value = "@" + lit.Language;
                }
                else
                {
                    if (cmd.Parameters.Contains(prefix + "Meta")) cmd.Parameters.RemoveAt(prefix + "Meta");
                }
            }
            else
            {
                if (cmd.Parameters.Contains(prefix + "Meta")) cmd.Parameters.RemoveAt(prefix + "Meta");
            }
        }

        internal INode DecodeNode(IGraph g, byte type, String value, String meta)
        {
            switch (type)
            {
                case 0:
                    return g.CreateBlankNode(value.Substring(2));
                case 1:
                    return g.CreateUriNode(new Uri(value));
                case 2:
                    if (meta == null)
                    {
                        return g.CreateLiteralNode(value);
                    }
                    else if (meta.StartsWith("@"))
                    {
                        return g.CreateLiteralNode(value, meta.Substring(1));
                    }
                    else
                    {
                        return g.CreateLiteralNode(value, new Uri(meta));
                    }
                default:
                    throw new NotSupportedException("Only Blank, URI and Literal Nodes are currently supported");
            }
        }

        internal String DecodeMeta(Object meta)
        {
            if (Convert.IsDBNull(meta))
            {
                return null;
            }
            else
            {
                return (String)meta;
            }
        }

        #endregion

        #region Dispose Logic

        /// <summary>
        /// Disposes of the Store Manager
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Disposes of the Store Manager
        /// </summary>
        /// <param name="disposing">Whether this was invoked by the Dispose() method (if not was invoked by the Finalizer)</param>
        private void Dispose(bool disposing)
        {
            if (disposing) GC.SuppressFinalize(this);

            this.DisposeInternal();
            if (this._connection != null)
            {
                try
                {
                    this._connection.Close();
                }
                catch
                {
                    //Suppress errors closing the connection
                }
            }
        }

        /// <summary>
        /// Does any additional dispose actions required by derived implementations
        /// </summary>
        /// <remarks>
        /// Will be called <em>before</em> the Connection is closed so derived implementations may
        /// </remarks>
        protected virtual void DisposeInternal()
        {

        }

        #endregion

        #region IGenericIOManager Members

        public void LoadGraph(IGraph g, Uri graphUri)
        {
            //First need to get the Graph ID (if any)
            TCommand cmd = this.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetGraphID";
            cmd.Connection = this._connection;
            if (graphUri != null)
            {
                cmd.Parameters.Add(this.GetParameter("graphUri"));
                cmd.Parameters["graphUri"].DbType = DbType.String;
                cmd.Parameters["graphUri"].Value = graphUri.ToString();
            }
            cmd.Parameters.Add(this.GetParameter("RC"));
            cmd.Parameters["RC"].DbType = DbType.Int32;
            cmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();

            int id = (int)cmd.Parameters["RC"].Value;

            if (id > 0)
            {
                //We got an ID so can start the load process
                //Set the Target Graph
                IGraph target = (g.IsEmpty) ? g : new Graph();

                cmd = this.GetCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "GetQuadsData";
                cmd.Connection = this._connection;
                cmd.Parameters.Add(this.GetParameter("graphID"));
                cmd.Parameters["graphID"].DbType = DbType.Int32;
                cmd.Parameters["graphID"].Value = id;

                using (DbDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            INode s = this.DecodeNode(target, (byte)reader["subjectType"], (String)reader["subjectValue"], this.DecodeMeta(reader["subjectMeta"]));
                            INode p = this.DecodeNode(target, (byte)reader["predicateType"], (String)reader["predicateValue"], this.DecodeMeta(reader["predicateMeta"]));
                            INode o = this.DecodeNode(target, (byte)reader["objectType"], (String)reader["objectValue"], this.DecodeMeta(reader["objectMeta"]));

                            target.Assert(new Triple(s, p, o));
                        }
                    }
                    reader.Close();
                    reader.Dispose();
                }

                if (!ReferenceEquals(target, g))
                {
                    g.Merge(target);
                }
            }
        }

        public void LoadGraph(IGraph g, string graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                this.LoadGraph(g, (Uri)null);
            }
            else
            {
                this.LoadGraph(g, new Uri(graphUri));
            }
        }

        public void SaveGraph(IGraph g)
        {
            //First need to get/create the Graph ID (if any)
            TCommand cmd = this.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GetOrCreateGraphID";
            cmd.Connection = this._connection;
            if (g.BaseUri != null)
            {
                cmd.Parameters.Add(this.GetParameter("graphUri"));
                cmd.Parameters["graphUri"].DbType = DbType.String;
                cmd.Parameters["graphUri"].Value = g.BaseUri.ToString();
            }
            cmd.Parameters.Add(this.GetParameter("RC"));
            cmd.Parameters["RC"].DbType = DbType.Int32;
            cmd.Parameters["RC"].Direction = ParameterDirection.ReturnValue;
            cmd.ExecuteNonQuery();

            int id = (int)cmd.Parameters["RC"].Value;

            if (id > 0)
            {
                //Then we need to ensure that all Quads associated with the Graph currently are removed
                cmd = this.GetCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "ClearGraph";
                cmd.Connection = this._connection;
                cmd.Parameters.Add(this.GetParameter("graphID"));
                cmd.Parameters["graphID"].DbType = DbType.Int32;
                cmd.Parameters["graphID"].Value = id;
                cmd.ExecuteNonQuery();

                //Then we can insert the triples
                cmd = this.GetCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "AssertQuadData";
                cmd.Connection = this._connection;
                cmd.Parameters.Add(this.GetParameter("graphID"));
                cmd.Parameters["graphID"].DbType = DbType.Int32;
                cmd.Parameters["graphID"].Value = id;

                foreach (Triple t in g.Triples)
                {
                    this.EncodeNode(cmd, t.Subject, TripleSegment.Subject);
                    this.EncodeNode(cmd, t.Predicate, TripleSegment.Predicate);
                    this.EncodeNode(cmd, t.Object, TripleSegment.Object);

                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                throw new RdfStorageException("Unable to Save a Graph as the underlying Store failed to generate a Graph ID");
            }
        }

        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new NotImplementedException();
        }

        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            throw new NotImplementedException();
        }

        public bool UpdateSupported
        {
            get { throw new NotImplementedException(); }
        }

        public void DeleteGraph(Uri graphUri)
        {
            //Delete the Graph by URI
            TCommand cmd = this.GetCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "DeleteGraphByUri";
            cmd.Connection = this._connection;
            if (graphUri != null)
            {
                cmd.Parameters.Add(this.GetParameter("graphUri"));
                cmd.Parameters["graphUri"].DbType = DbType.String;
                cmd.Parameters["graphUri"].Value = graphUri.ToString();
            }
            cmd.ExecuteNonQuery();
        }

        public void DeleteGraph(string graphUri)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                this.DeleteGraph((Uri)null);
            }
            else
            {
                this.DeleteGraph(new Uri(graphUri));
            }
        }

        public bool DeleteSupported
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Uri> ListGraphs()
        {
            throw new NotImplementedException();
        }

        public bool ListGraphsSupported
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReady
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }

    public abstract class BaseAdoSqlClientStore
        : BaseAdoStore<SqlConnection, SqlCommand, SqlParameter, SqlDataAdapter, SqlException>
    {
        public BaseAdoSqlClientStore(SqlConnection connection)
            : base(connection) { }
    }
}
