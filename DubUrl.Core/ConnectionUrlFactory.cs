﻿using DubUrl.Mapping;
using DubUrl.Parsing;
using DubUrl.Querying.Dialects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DubUrl
{
    public class ConnectionUrlFactory
    {
        private SchemeMapperBuilder SchemeMapperBuilder { get; }
        private IParser Parser { get; }


        public ConnectionUrlFactory(SchemeMapperBuilder builder)
            : this(new Parser(), builder) { }

        internal ConnectionUrlFactory(IParser parser, SchemeMapperBuilder builder)
            => (Parser, SchemeMapperBuilder) = (parser, builder);

        public ConnectionUrl Instantiate(string url)
            => new(url, Parser, SchemeMapperBuilder);
    }
}
