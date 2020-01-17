using System.Collections.Generic;

namespace Jerrycurl.Extensions.EntityFrameworkCore.Test.Entities
{
    public partial class Address
    {
        public Address()
        {
            OrderBillingAddress = new HashSet<Order>();
            OrderShippingAddress = new HashSet<Order>();
        }

        public int Id { get; set; }
        public string Street { get; set; }

        public virtual ICollection<Order> OrderBillingAddress { get; set; }
        public virtual ICollection<Order> OrderShippingAddress { get; set; }
    }
}
