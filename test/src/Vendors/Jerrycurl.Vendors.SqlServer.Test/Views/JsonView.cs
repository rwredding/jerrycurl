using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Vendors.SqlServer.Test.Models;

namespace Jerrycurl.Vendors.SqlServer.Test.Views
{
    [Table]
    public class JsonView
    {
        public JsonModel Json { get; set; }
        public string JsonString { get; set; }
    }
}
