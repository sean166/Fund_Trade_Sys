using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fund_Trade_Sys.Models
{
    public class InvestPerformance
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string FundCode { get; set; }
        public double Gain { get; set; }
        public double Loss { get; set; }
        public double UnrealizedGain { get; set; }
        public double UnrealizedLoss { get; set; }
        public double Cost { get; set; }
        public double GainPercentage { get; set; }
        public double LossPercentage { get; set; }
        public int AccountId { get; set; }
    }
}