using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fund_Trade_Sys.Models
{
    public class SECClientAccount
    {
        public int AccountId { get; set; }
        public int AccountNum { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public double Balance { get; set; }
        public string AccountStatus { get; set; }
        public int ClientId { get; set; }

    }
}