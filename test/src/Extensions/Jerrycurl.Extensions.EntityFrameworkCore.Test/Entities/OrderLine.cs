using System;
using System.Collections.Generic;
using System.Text;

namespace Jerrycurl.Extensions.EntityFrameworkCore.Test.Entities
{
    public partial class OrderLine
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Product { get; set; }

        public virtual Order Order { get; set; }
    }
}
