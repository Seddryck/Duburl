﻿using DubUrl.Querying.Dialects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DubUrl.Mapping.Database;

[Database<MsAccessDialect>(
    "Microsoft Access"
    , ["accdb", "access", "msaccess"]
    , DatabaseCategory.FileBased
)]
[Brand("microsoftaccess", "#217346")]
public class MsAccessDatabase : IDatabase
{ }
