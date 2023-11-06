﻿using DubUrl.Locating.RegexUtils;
using DubUrl.Mapping.Database;
using DubUrl.OleDb.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DubUrl.Mapping;
using DubUrl.Rewriting.Tokening;

namespace DubUrl.OleDb.Providers
{
    [Provider<MssqlOleDbProviderRegex, OleDbMapper, MsSqlServerDatabase>()]
    internal class MssqlOleDbProviderLocator : BaseProviderLocator
    {
        internal class MssqlOleDbProviderRegex : BaseProviderRegex
        {
            public MssqlOleDbProviderRegex()
                : base(new BaseRegex[]
                {
                    new WordMatch("MSOLEDBSQL"),
                })
            { }
        }
        private List<string> Candidates { get; } = new();

        public MssqlOleDbProviderLocator()
            : base(GetRegexPattern<MssqlOleDbProviderLocator>(), new BaseTokenMapper[]
                { new OptionsMapper()
                    , new OleDbRewriter.InitialCatalogMapper()
                    , new OleDbRewriter.ServerMapper()
                }
            )
        { }

        internal MssqlOleDbProviderLocator(ProviderLister providerLister)
            : base(GetRegexPattern<MssqlOleDbProviderLocator>(), providerLister) { }

        internal MssqlOleDbProviderLocator(string value)
            : base(GetRegexPattern<MssqlOleDbProviderLocator>(), new BaseTokenMapper[]
                { new OptionsMapper()
                    , new OleDbRewriter.InitialCatalogMapper()
                    , new OleDbRewriter.ServerMapper()
                }
            ) { }

        protected override void AddCandidate(string provider, string[] matches)
            => Candidates.Add(provider);
        protected override List<string> RankCandidates()
            => Candidates;
    }
}
