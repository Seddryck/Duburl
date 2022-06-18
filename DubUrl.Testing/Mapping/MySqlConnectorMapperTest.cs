﻿using DubUrl.Mapping;
using DubUrl.Parsing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;

namespace DubUrl.Testing.Mapping
{
    public class MySqlConnectorMapperTest
    {
        private const string PROVIDER_NAME = "Mysql";

        private DbConnectionStringBuilder ConnectionStringBuilder
        {
            get => ConnectionStringBuilderHelper.Retrieve(PROVIDER_NAME, MySqlConnectorFactory.Instance);
        }

        [Test]
        [TestCase("host", "host")]
        [TestCase("host", "host", "db", 1234)]
        public void UrlInfo_Map_DataSource(string expected, string host = "host", string segmentsList = "db", int port = 0)
        {
            var urlInfo = new UrlInfo() { Host = host, Port = port, Segments = segmentsList.Split('/'), Username = "user" };
            var mapper = new MySqlConnectorMapper(ConnectionStringBuilder);
            var result = mapper.Map(urlInfo);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.ContainKey("Server"));
            Assert.That(result["Server"], Is.EqualTo(expected));
        }

        [Test]
        [TestCase("db")]
        public void UrlInfo_Map_InitialCatalog(string segmentsList = "db", string expected = "db")
        {
            var urlInfo = new UrlInfo() { Segments = segmentsList.Split('/'), Username = "user" };
            var mapper = new MySqlConnectorMapper(ConnectionStringBuilder);
            var result = mapper.Map(urlInfo);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.ContainKey("Database"));
            Assert.That(result["Database"], Is.EqualTo(expected));
        }


        [Test]
        public void UrlInfoWithUsernamePassword_Map_Authentication()
        {
            var urlInfo = new UrlInfo() { Username = "user", Password = "pwd", Segments = new[] { "db" } };
            var mapper = new MySqlConnectorMapper(ConnectionStringBuilder);
            var result = mapper.Map(urlInfo);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.ContainKey("User ID"));
            Assert.That(result["User ID"], Is.EqualTo("user"));
            Assert.That(result, Does.ContainKey("Password"));
            Assert.That(result["Password"], Is.EqualTo("pwd"));
            Assert.That(result, Does.Not.ContainKey("Integrated Security"));
        }

        [Test]
        public void UrlInfoWithoutUsernamePassword_Map_Authentication()
        {
            var urlInfo = new UrlInfo() { Username = "", Password = "", Segments = new[] { "db" } };
            var mapper = new MySqlConnectorMapper(ConnectionStringBuilder);
            Assert.Catch<UsernameNotFoundException>(() => mapper.Map(urlInfo));
        }

        [Test]
        public void UrlInfo_Map_Options()
        {
            var urlInfo = new UrlInfo() { Username="user", Segments = new[] { "db" } };
            urlInfo.Options.Add("Application Name", "myApp");
            urlInfo.Options.Add("Persist Security Info", "true");

            var mapper = new MySqlConnectorMapper(ConnectionStringBuilder);
            var result = mapper.Map(urlInfo);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.ContainKey("Application Name"));
            Assert.That(result["Application Name"], Is.EqualTo("myApp"));
            Assert.That(result, Does.ContainKey("Persist Security Info"));
            Assert.That(result["Persist Security Info"], Is.True);
        }
    }
}
