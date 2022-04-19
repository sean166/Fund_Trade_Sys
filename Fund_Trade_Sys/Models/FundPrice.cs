using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fund_Trade_Sys.Models
{
    public class FundPrice
    {
        public int PriceId { get; set; }
        public double Price { get; set; }
        public int FundId { get; set; }
        public string FundCode { get; set; }
        public DateTime PriceDate { get; set; }
    }
}