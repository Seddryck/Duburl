﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DubUrl.Querying.Dialects.Formatters
{
    internal class NullFormatter : INullFormatter
    {
        public string Format()
            => "NULL";
    }
}