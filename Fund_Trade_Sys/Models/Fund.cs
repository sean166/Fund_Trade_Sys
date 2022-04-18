using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fund_Trade_Sys.Models
{
    public class Fund
    {
        public int FindId { get; set; }
        public string FundName { get; set; }
        public string FundCode { get; set; }
        public DateTime PriceTime { get; set; }
        public double Price { get; set; }
    }
}