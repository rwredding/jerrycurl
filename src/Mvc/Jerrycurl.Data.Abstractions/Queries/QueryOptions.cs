﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jerrycurl.Data;
using Jerrycurl.Data.Filters;
using Jerrycurl.Data.Sessions;
using Jerrycurl.Relations.Metadata;

namespace Jerrycurl.Data.Queries
{
    public class QueryOptions : SessionOptions
    {
        public ISchemaStore Schemas { get; set; }
    }
}
