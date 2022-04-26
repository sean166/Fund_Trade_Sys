using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fund_Trade_Sys.Models
{
    public class FundPerformance
    {
        public DateTime FromeDate { get; set; }
        public DateTime ToDate { get; set; }
        public string FundCode { get; set; }
        public double Profits { get; set; }
        public double ProfitsPercentage { get; set; }
    }
}