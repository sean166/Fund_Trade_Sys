using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fund_Trade_Sys.Models
{
    public class Margin
    {
        public int MarginId { get; set; }
        public DateTime MarginDate { get; set; }
        public string FundCode { get; set; }
        public double Profit { get; set; }
        public double CurAvePrice { get; set; }
        public double  MarketPrice { get; set; }
        public double CurAmount { get; set; }
        public int HoldUnits { get; set; }
        public int TradeUnits { get; set; }

    }
}