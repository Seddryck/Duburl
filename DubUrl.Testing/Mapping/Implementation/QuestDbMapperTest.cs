﻿using DubUrl.Mapping.Implementation;
using DubUrl.Querying.Dialecting;
using DubUrl.Querying.Parametrizing;
using DubUrl.Testing.Rewriting;
using Npgsql;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DubUrl.Testing.Mapping.Implementation
{
    public class QuestDbMapperTest
    {
        private const string PROVIDER_NAME = "Npgsql";

        private static DbConnectionStringBuilder ConnectionStringBuilder
        {
            get => ConnectionStringBuilderHelper.Retrieve(PROVIDER_NAME, NpgsqlFactory.Instance);
        }

        [Test]
        public void GetDialect_None_DialectReturned()
        {
            var mapper = new QuestDbMapper(ConnectionStringBuilder, new QuestDbDialect(new[] { "quest", "questdb" }), new PositionalParametrizer());
            var result = mapper.GetDialect();

            Assert.That(result, Is.Not.Null.Or.Empty);
            Assert.IsInstanceOf<QuestDbDialect>(result);
            Assert.That(result.Aliases, Does.Contain("quest"));
            Assert.That(result.Aliases, Does.Contain("questdb"));
        }
    }
}