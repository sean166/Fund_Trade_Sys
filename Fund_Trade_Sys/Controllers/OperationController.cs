using Fund_Trade_Sys.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace Fund_Trade_Sys.Controllers
{
    public class OperationController : ApiController
    {
        string connection = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
        [HttpGet]
        public string Test(string name)
        {
            return "Welcome" + name;
        }
           
        [HttpGet]
        public IHttpActionResult getClient()
        {
            List<Client> client = new List<Client>();
            //string connection = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
            SqlConnection sqlConnection = new SqlConnection(connection);
            string query = "select * from [dbo].[tblClient]";
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            SqlDataReader reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                client.Add(new Client() { 
                    FirstName = reader["first_name"].ToString(),
                    LastNmae = reader["last_name"].ToString(),
                    Age = Convert.ToInt32(reader["age"]),
                    Gender = reader["gender"].ToString()

                });    
            }
            sqlConnection.Close();
            return Ok(client);

        }

        [HttpGet]
        public IHttpActionResult getFundListandPrice()
        {
            List<Fund> funds = new List<Fund>();
            
            SqlConnection sqlConnection = new SqlConnection(connection);
            string query = "SELECT i.fund_name, s.fund_code,price, price_date FROM securities AS s JOIN instrument AS i ON s.fund_id = i.fund_id";
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            SqlDataReader reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                funds.Add(new Fund()
                {
                    FundName = reader["fund_name"].ToString(),
                    FundCode = reader["fund_code"].ToString(),
                    Price = Convert.ToDouble(reader["price"]),
                    PriceTime = (DateTime)reader["price_date"]

                });
            }
            sqlConnection.Close();
            return Ok(funds);

        }

        [HttpPost]
        public IHttpActionResult CreateDeposit(double money,int accountId)
        {
            CashFlow cashFlow = new CashFlow()
            {
                AccountId = accountId,
                MoneyIn = money,
            };
            string query = " insert into cashEntry values (@money_in,@money_out,@account_id)";
            query += "SELECT SCOPE_IDENTITY()";
            using(SqlConnection sqlConnection = new SqlConnection(connection))
            {
                using(SqlCommand sqlCommand = new SqlCommand(query))
                {
                    sqlCommand.Parameters.AddWithValue("@money_in", cashFlow.MoneyIn);
                    sqlCommand.Parameters.AddWithValue("@account_id", cashFlow.AccountId);
                    sqlCommand.Parameters.AddWithValue("@money_out", cashFlow.MoneyOut);
                    sqlCommand.Connection = sqlConnection;
                    sqlConnection.Open();
                    cashFlow.CashFlowId=Convert.ToInt32(sqlCommand.ExecuteScalar());
                    sqlConnection.Close();
                }
            }



            //ring query2 = ""
            return Ok(cashFlow);
        }

      
    }
}
