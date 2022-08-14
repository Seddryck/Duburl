﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using DubUrl.Mapping.Tokening;
using DubUrl.Mapping;
using System.Data.Common;
using DubUrl.Parsing;
using DubUrl.Locating.OdbcDriver;
using Moq;
using System.Runtime.InteropServices;
using DubUrl.Mapping.Implementation;
using DubUrl.Querying.Dialecting;

namespace DubUrl.Testing.Mapping
{
    public class SchemeMapperBuilderTest
    {
        [SetUp]
        public void DefaultRegistration()
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", System.Data.SqlClient.SqlClientFactory.Instance);
            DbProviderFactories.RegisterFactory("Npgsql", Npgsql.NpgsqlFactory.Instance);
            DbProviderFactories.RegisterFactory("MySqlConnector", MySqlConnector.MySqlConnectorFactory.Instance);
            DbProviderFactories.RegisterFactory("Oracle.ManagedDataAccess", Oracle.ManagedDataAccess.Client.OracleClientFactory.Instance);
            DbProviderFactories.RegisterFactory("Microsoft.Data.Sqlite", Microsoft.Data.Sqlite.SqliteFactory.Instance);
            DbProviderFactories.RegisterFactory("IBM.Data.DB2.Core", IBM.Data.DB2.Core.DB2Factory.Instance);
            DbProviderFactories.RegisterFactory("Snowflake.Data", Snowflake.Data.Client.SnowflakeDbFactory.Instance);
            DbProviderFactories.RegisterFactory("Teradata.Client", Teradata.Client.Provider.TdFactory.Instance);
            DbProviderFactories.RegisterFactory("FirebirdSql.Data.FirebirdClient", FirebirdSql.Data.FirebirdClient.FirebirdClientFactory.Instance);
            DbProviderFactories.RegisterFactory("System.Data.Odbc", System.Data.Odbc.OdbcFactory.Instance);
#pragma warning disable CA1416 // Validate platform compatibility
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                DbProviderFactories.RegisterFactory("System.Data.OleDb", System.Data.OleDb.OleDbFactory.Instance);
#pragma warning restore CA1416 // Validate platform compatibility
        }

        private class StubMapper : BaseMapper
        {
            public StubMapper(DbConnectionStringBuilder csb, IDialect dialect) : base(csb, dialect, new Specificator(csb), Array.Empty<BaseTokenMapper>()) { }
        }

        [Test]
        [TestCase("oracle", typeof(OracleManagedDataAccessMapper))]
        [TestCase("mysql", typeof(MySqlConnectorMapper))]
        [TestCase("mssql", typeof(MssqlMapper))]
        [TestCase("pgsql", typeof(PgsqlMapper))]
        [TestCase("db2", typeof(Db2Mapper))]
        [TestCase("sqlite", typeof(SqliteMapper))]
        [TestCase("maria", typeof(MariaDbConnectorMapper))]
        [TestCase("sf", typeof(SnowflakeMapper))]
        [TestCase("td", typeof(TeradataMapper))]
        [TestCase("fb", typeof(FirebirdSqlMapper))]
        [TestCase("cr", typeof(CockRoachMapper))]
        [TestCase("ts", typeof(TimescaleMapper))]
        [TestCase("odbc", typeof(OdbcMapper))]
        [TestCase("odbc+mssql", typeof(OdbcMapper))]
        [TestCase("mssql+odbc", typeof(OdbcMapper))]
        [TestCase("oledb", typeof(OleDbMapper))]
        [TestCase("oledb+mssql", typeof(OleDbMapper))]
        [TestCase("mssql+oledb", typeof(OleDbMapper))]
        public void Instantiate_Scheme_CorrectType(string schemeList, Type expected)
        {
            var builder = new SchemeMapperBuilder();
            builder.Build();
            var result = builder.GetMapper(schemeList.Split(new[] { '+', ':' }));

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf(expected));
        }

        [Test]
        public void AddAlias_NewScheme_CorrectType()
        {
            var weirdScheme = "xyz";

            var builder = new SchemeMapperBuilder();
            builder.Build();
            Assert.Catch<SchemeNotFoundException>(() => builder.GetMapper(weirdScheme)); //Should not exists

            builder.AddAlias(weirdScheme, "mssql");
            builder.Build();
            var result = builder.GetMapper(weirdScheme); //Should exists
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<MssqlMapper>());
        }

        [Test]
        public void AddMapping_NewScheme_CorrectType()
        {
            (var weirdScheme, var invariantName) = ("xyz", "x.y.z");

            var builder = new SchemeMapperBuilder();
            builder.Build();
            Assert.Catch<SchemeNotFoundException>(() => builder.GetMapper(weirdScheme)); //Should not exists

            DbProviderFactories.RegisterFactory(invariantName, System.Data.SqlClient.SqlClientFactory.Instance);
            builder.AddMapping(typeof(StubMapper), invariantName, new[] { weirdScheme }, typeof(AnsiDialect));

            builder.Build();
            var result = builder.GetMapper(weirdScheme); //Should exists
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<StubMapper>());
        }

        [Test]
        public void RemoveMapping_ExistingScheme_NotFound()
        {
            var oracleScheme = "ora";

            var builder = new SchemeMapperBuilder();
            builder.Build();
            var result = builder.GetMapper(oracleScheme); //should be found
            Assert.That(result, Is.Not.Null);

            builder.RemoveMapping("Oracle.ManagedDataAccess");
            builder.Build();
            Assert.Catch<SchemeNotFoundException>(() => builder.GetMapper(oracleScheme)); //Should not exist
        }

        [Test]
        public void ReplaceMapper_NewScheme_CorrectType()
        {
            var mysqlScheme = "mysql";

            var builder = new SchemeMapperBuilder();
            if(!DbProviderFactories.GetProviderInvariantNames().Contains("MySql.Data"))
                DbProviderFactories.RegisterFactory("MySql.Data", MySql.Data.MySqlClient.MySqlClientFactory.Instance);
            builder.ReplaceMapper(typeof(MySqlConnectorMapper), typeof(MySqlDataMapper));

            builder.Build();
            var result = builder.GetMapper(mysqlScheme);
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<MySqlDataMapper>());
        }

        private class FakeDriverLocator : IDriverLocator
        {
            public FakeDriverLocator() { }

            public string Locate() => "ODBC Driver for fooBar";
        }

        [Test]
        public void ReplaceDriverLocationFactory_NewDriverLocationFactory_CorrectType()
        {
            var factory = new DriverLocatorFactory();
            factory.AddDriver("foobar", typeof(FakeDriverLocator));

            var builder = new SchemeMapperBuilder();
            DbProviderFactories.RegisterFactory("System.Data.Odbc", System.Data.Odbc.OdbcFactory.Instance);
            builder.ReplaceDriverLocatorFactory(typeof(OdbcMapper), factory);

            builder.Build();
            var result = builder.GetMapper(new[] { "odbc", "foobar" });
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.TypeOf<OdbcMapper>());
            Assert.That(((OdbcMapper)result).DriverLocatorFactory, Is.EqualTo(factory));
        }
    }
}
