using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Data.Metadata.Annotations;
using Jerrycurl.Mvc.Metadata.Annotations;
using Newtonsoft.Json;

namespace Jerrycurl.Vendors.Postgres.Test.Models
{
    [Table]
    public class TestModel
    {
        public JsonModel Json { get; set; }
        public JsonModel JsonB { get; set; }
    }
}
