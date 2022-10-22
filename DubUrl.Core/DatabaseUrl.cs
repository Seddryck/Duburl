﻿using DubUrl.Mapping;
using DubUrl.MicroOrm;
using DubUrl.Querying;
using DubUrl.Querying.Parametrizing;
using DubUrl.Querying.Reading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DubUrl
{
    public partial class DatabaseUrl
    {
        protected ConnectionUrl ConnectionUrl { get; }
        private CommandProvisionerFactory CommandProvisionerFactory { get; }

        public DatabaseUrl(string url)
        : this(new ConnectionUrlFactory(new SchemeMapperBuilder()), url)
        { }

        public DatabaseUrl(ConnectionUrlFactory factory, string url)
            : this(factory.Instantiate(url), new CommandProvisionerFactory())
        { }

        public DatabaseUrl(ConnectionUrlFactory factory, CommandProvisionerFactory commandProvisionerFactory, string url)
            : this(factory.Instantiate(url), commandProvisionerFactory)
        { }

        internal DatabaseUrl(ConnectionUrl connectionUrl, CommandProvisionerFactory commandProvisionerFactory)
            => (ConnectionUrl, CommandProvisionerFactory) = (connectionUrl, commandProvisionerFactory);

        protected virtual IDbCommand PrepareCommand(ICommandProvider commandProvider)
        {
            var conn = ConnectionUrl.Open();
            var cmd = conn.CreateCommand();
            var provisioners = CommandProvisionerFactory.Instantiate(commandProvider, ConnectionUrl);
            foreach (var provisioner in provisioners)
                provisioner.Execute(cmd);
            return cmd;
        }


        #region Scalar

        public object? ReadScalar(string query)
            => ReadScalar(new InlineCommand(query));

        public object? ReadScalar(ICommandProvider commandProvider)
            => PrepareCommand(commandProvider).ExecuteScalar();

        public object ReadScalarNonNull(string query)
            => ReadScalarNonNull(new InlineCommand(query));

        public object ReadScalarNonNull(ICommandProvider commandProvider)
        {
            var result = ReadScalar(commandProvider);
            var typedResult = result == DBNull.Value ? null : result;
            return typedResult ?? throw new NullReferenceException();
        }

        public T? ReadScalar<T>(string query)
          => ReadScalar<T>(new InlineCommand(query));

        public T? ReadScalar<T>(ICommandProvider query)
        {
            var result = ReadScalar(query);
            return (T?)(result == DBNull.Value ? null : result);
        }

        public T ReadScalarNonNull<T>(string query)
           => ReadScalarNonNull<T>(new InlineCommand(query));

        public T ReadScalarNonNull<T>(ICommandProvider query)
            => (T)ReadScalarNonNull(query);


        #endregion

        #region Single

        protected (object?, IDataReader) ReadInternal(ICommandProvider commandProvider)
        {
            using var dr = PrepareCommand(commandProvider).ExecuteReader();
            if (!dr.Read())
                return (null, dr);
            return (dr.ToExpandoObject(), dr);
        }

        public object? ReadSingle(string query)
            => ReadSingle(new InlineCommand(query));

        public object? ReadSingle(ICommandProvider commandProvider)
        {
            (var dyn, var dr) = ReadInternal(commandProvider);
            return !dr.Read() ? dyn : throw new InvalidOperationException();
        }

        public object ReadSingleNonNull(string query)
            => ReadSingleNonNull(new InlineCommand(query));

        public object ReadSingleNonNull(ICommandProvider commandProvider)
            => ReadSingle(commandProvider) ?? throw new InvalidOperationException();

        #endregion

        #region First 

        public object? ReadFirst(string query)
            => ReadFirst(new InlineCommand(query));

        public object? ReadFirst(ICommandProvider commandProvider)
        {
            (var dyn, _) = ReadInternal(commandProvider);
            return dyn;
        }

        public object ReadFirstNonNull(string query)
            => ReadFirstNonNull(new InlineCommand(query));

        public object ReadFirstNonNull(ICommandProvider commandProvider)
            => ReadFirst(commandProvider) ?? throw new InvalidOperationException();

        #endregion

        #region Multiple

        public IEnumerable<object> ReadMultiple(string query)
            => ReadMultiple(new InlineCommand(query));

        public IEnumerable<object> ReadMultiple(ICommandProvider commandProvider)
        {
            using var dr = PrepareCommand(commandProvider).ExecuteReader();
            while (dr.Read())
                yield return dr.ToExpandoObject();
            dr?.Close();
        }

        #endregion

        #region ExecuteReader

        public IDataReader ExecuteReader(string query)
           => ExecuteReader(new InlineCommand(query));

        public IDataReader ExecuteReader(ICommandProvider commandProvider)
            => PrepareCommand(commandProvider).ExecuteReader();

        #endregion
    }
}

