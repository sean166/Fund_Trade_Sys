using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fund_Trade_Sys.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public double Amount { get; set; }
        public int Units { get; set; }
        public double Price { get; set; }
        public DateTime OrderTime { get; set; }
        public string OrderStatus { get; set; }
        public int AcountId { get; set; }
    }
}