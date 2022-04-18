using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fund_Trade_Sys.Models
{
    public class CashFlow
    {
        
        public int CashFlowId { get; set; }
        public double MoneyIn { get; set; }
        public double MoneyOut { get; set; }
        public int AccountId { get; set; }
    }
}