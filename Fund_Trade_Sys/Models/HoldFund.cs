using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fund_Trade_Sys.Models
{
    public class HoldFund
    {
        public int HoldId { get; set; }
        public double AveragePrice { get; set; }
        public double Amount { get; set; }
        public int Unit { get; set; }
        public string FundCode { get; set; }
        public int AccountId { get; set; }
    }
}