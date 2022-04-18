using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Fund_Trade_Sys.Models
{
    public class Client
    {
        public int ClientId { get; set; }
        public string LastNmae { get; set; }
        public string FirstName { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
    }
}