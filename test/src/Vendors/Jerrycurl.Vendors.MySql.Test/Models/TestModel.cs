using System;
using System.Collections.Generic;
using System.Text;
using Jerrycurl.Mvc.Metadata.Annotations;
using Newtonsoft.Json;

namespace Jerrycurl.Vendors.MySql.Test.Models
{
    public class TestModel
    {
        public JsonModel Json { get; set; }
        public string JsonString { get; set; }
    }
}
