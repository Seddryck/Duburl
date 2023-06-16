﻿using DubUrl.Querying.Dialects.Formatters;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DubUrl.Testing.Querying.Dialects.Formatters
{
    public class IntervalTrinoFormatterTest
    {
        [Test]
        [TestCase("2.00:00:00", "INTERVAL '2' DAY")]
        [TestCase("06:00:00", "INTERVAL '6' HOUR")]
        [TestCase("06:12:00", "INTERVAL '6' HOUR + INTERVAL '12' MINUTE")]
        [TestCase("06:00:30", "INTERVAL '6' HOUR + INTERVAL '30' SECOND")]
        [TestCase("2.05:00:00", "INTERVAL '2' DAY + INTERVAL '5' HOUR")]
        [TestCase("2.05:17:42", "INTERVAL '2' DAY + INTERVAL '5' HOUR + INTERVAL '17' MINUTE + INTERVAL '42' SECOND")]
        public void Format_Interval_Match(string ts, string expected)
            => Assert.That(new IntervalTrinoFormatter().Format(TimeSpan.Parse(ts)), Is.EqualTo(expected));
    }
}
