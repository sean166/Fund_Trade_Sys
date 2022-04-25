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
            UpdateDailyPrice(); //test purpose need to delete later
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



            string query2 = "UPDATE SECClientAccount SET  balance = balance + @balance Where account_id = @account_id ";
            using (SqlConnection sqlConnection1 = new SqlConnection(connection))
            {
                using( SqlCommand sqlCommand2 = new SqlCommand(query2))
                {
                    sqlCommand2.Parameters.AddWithValue("@account_id", cashFlow.AccountId);
                    sqlCommand2.Parameters.AddWithValue("@balance",cashFlow.MoneyIn);
                    sqlCommand2.Connection = sqlConnection1;
                    sqlConnection1.Open();
                    sqlCommand2.ExecuteNonQuery();
                    sqlConnection1.Close();
                }
            }

                return Ok(cashFlow);
        }

        [HttpPost]
        public IHttpActionResult MoneyWithdraw(double money, int accountId)
        {
            CashFlow cashFlow = new CashFlow()
            {
                AccountId = accountId,
                MoneyOut = money,
            };
            string query = " insert into cashEntry values (@money_in,@money_out,@account_id)";
            query += "SELECT SCOPE_IDENTITY()";
            using (SqlConnection sqlConnection = new SqlConnection(connection))
            {
                using (SqlCommand sqlCommand = new SqlCommand(query))
                {
                    sqlCommand.Parameters.AddWithValue("@money_in", cashFlow.MoneyIn);
                    sqlCommand.Parameters.AddWithValue("@account_id", cashFlow.AccountId);
                    sqlCommand.Parameters.AddWithValue("@money_out", cashFlow.MoneyOut);
                    sqlCommand.Connection = sqlConnection;
                    sqlConnection.Open();
                    cashFlow.CashFlowId = Convert.ToInt32(sqlCommand.ExecuteScalar());
                    sqlConnection.Close();
                }
            }



            string query2 = "UPDATE SECClientAccount SET  balance = balance - @balance Where account_id = @account_id ";
            using (SqlConnection sqlConnection1 = new SqlConnection(connection))
            {
                using (SqlCommand sqlCommand2 = new SqlCommand(query2))
                {
                    sqlCommand2.Parameters.AddWithValue("@account_id", cashFlow.AccountId);
                    sqlCommand2.Parameters.AddWithValue("@balance", cashFlow.MoneyOut);
                    sqlCommand2.Connection = sqlConnection1;
                    sqlConnection1.Open();
                    sqlCommand2.ExecuteNonQuery();
                    sqlConnection1.Close();
                }
            }

            return Ok(cashFlow);
        }

        [HttpGet]
        public IHttpActionResult GetBalance(int accountId)
        {
            SECClientAccount account = new SECClientAccount();
            string query = "Select balance From SECClientAccount Where account_id = " + accountId;
            
            SqlConnection sqlConnection = new SqlConnection(connection);
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            SqlDataReader reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                account.Balance = Convert.ToDouble(reader["balance"]);
            }
            sqlConnection.Close();
            

            return Ok(account.Balance);


        }


        //consider to add trans_date for sorting and filtering
        [Authorize]
        [HttpGet]
        public IHttpActionResult GetHistoryCashFlow(int accountId)
        {
            List<CashFlow> cashFlows = new List<CashFlow>();
            string query = "Select * From cashEntry Where account_id = " + accountId;
            SqlConnection sqlConnection = new SqlConnection(connection);
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            SqlDataReader reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                cashFlows.Add(new CashFlow()
                {
                    MoneyIn = Convert.ToDouble(reader["money_in"]),
                    MoneyOut = Convert.ToDouble(reader["money_out"]),
                    AccountId = Convert.ToInt32(reader["account_id"])
                });
            }
            sqlConnection.Close();
            return Ok(cashFlows);
        }

        [HttpPost]
        public IHttpActionResult CreateOrder(string fundCode, int unit, int accountId) 

        {
            bool IsWorkday = DateTime.Now.DayOfWeek != DayOfWeek.Sunday && DateTime.Now.DayOfWeek != DayOfWeek.Saturday;
            if (!IsWorkday)
            {
                return Ok("We don't work during weekends!");
            }
            //update the daily price first
            UpdateDailyPrice();
            int orderId = 0;
            DateTime today = DateTime.Now;
            var d = today.ToString("yyyy-MM-dd");
            SqlConnection sqlConnection = new SqlConnection(connection);

            //get fund  price
            string fundQuery = "Select Top 1 price From securities Where convert(date,price_date) = @today and fund_code = @fund_code";
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand(fundQuery, sqlConnection);
            sqlCommand.Parameters.AddWithValue("@today", d);
            sqlCommand.Parameters.AddWithValue("@fund_code",fundCode);           
            var price = Convert.ToDouble(sqlCommand.ExecuteScalar());
            sqlConnection.Close();

            //if price == 0 means the fund is not exist
            if(price == 0)
            {
                return Ok("The fund is not exist please double check your fund code");
            }
            //check account balance
            var amount = price * unit;
            string query = "Select balance From SECClientAccount Where account_id = " + accountId;
            sqlConnection.Open();
            SqlCommand cmd = new SqlCommand(query, sqlConnection);
            
            var balance = Convert.ToDouble(cmd.ExecuteScalar());
            sqlConnection.Close();
            if (balance > amount)
            {
                //update balance               
                string balanceQuery = "Update SECClientAccount Set balance = balance - @amount Where account_id = " + accountId;
                sqlConnection.Open();
                SqlCommand cmd2 = new SqlCommand(balanceQuery, sqlConnection);
                cmd2.Parameters.AddWithValue("@amount", amount);
                cmd2.ExecuteNonQuery();
                sqlConnection.Close();

                //create the order
                string orderQuery = "Insert INTO orders Values(@fund_name,@amount,@units,@price,@order_time,@order_status,@account_id)";
                orderQuery+= "SELECT SCOPE_IDENTITY()";
                sqlConnection.Open();
                SqlCommand cmd3 = new SqlCommand(orderQuery, sqlConnection);
                cmd3.Parameters.AddWithValue("fund_name",fundCode.ToString());
                cmd3.Parameters.AddWithValue("@amount", amount);
                cmd3.Parameters.AddWithValue("@units", unit);
                cmd3.Parameters.AddWithValue("@price", price);
                cmd3.Parameters.AddWithValue("@order_time", today);
                cmd3.Parameters.AddWithValue("@order_status", "Pending");
                cmd3.Parameters.AddWithValue("@account_id",accountId);
                orderId = Convert.ToInt32(cmd3.ExecuteScalar());
                sqlConnection.Close();

                //create/update client holding funds
                //check if user hold the funds before
                string checkQuery = "Select units From clientHoldFunds Where fund_name = @fund_name and account_id =  " + accountId;
                sqlConnection.Open();
                SqlCommand cmd4 = new SqlCommand(checkQuery, sqlConnection);
                cmd4.Parameters.AddWithValue("@fund_name", fundCode.ToString());
                var isHold = Convert.ToInt32(cmd4.ExecuteScalar());
                sqlConnection.Close();

                string checkQuery1 = "Select chf_id From clientHoldFunds Where fund_name = @fund_name and account_id =  " + accountId;
                sqlConnection.Open();
                SqlCommand cmd41 = new SqlCommand(checkQuery1, sqlConnection);
                cmd41.Parameters.AddWithValue("@fund_name", fundCode.ToString());
                var isExist = Convert.ToInt32(cmd41.ExecuteScalar());
                sqlConnection.Close();

                //if user wants to sell funds check about the holding shares are bigger or equal to the shares he wants to sell
                if (unit < 0 && isHold<-unit)
                {
                    return Ok("No enough shares to sell");
                }
               
                
                if (isExist > 0)//update the hold fund amount, units and the average price
                {
                    string updateFundQuery;
                    if (amount > 0)
                    {
                        updateFundQuery = " Update clientHoldFunds SET ave_price = (units*ave_price+@amount)/(units+@units),amount = amount+ @amount, units = units+@units WHERE fund_name = @fund_name and account_id = " + accountId;
                        sqlConnection.Open();
                        SqlCommand cmd6 = new SqlCommand(updateFundQuery, sqlConnection);
                        cmd6.Parameters.AddWithValue("@amount", amount);
                        cmd6.Parameters.AddWithValue("@units", unit);
                        cmd6.Parameters.AddWithValue("@fund_name", fundCode.ToString());
                        cmd6.ExecuteNonQuery();
                        sqlConnection.Close();
                    }
                    //amount < 0 means sell the funds and it doesn't affect the holding fund average price
                    else
                    {
                        updateFundQuery = " Update clientHoldFunds SET amount = (@units+units)*@price, units = units+@units WHERE fund_name = @fund_name and account_id = " + accountId;
                        sqlConnection.Open();
                        SqlCommand cmd6 = new SqlCommand(updateFundQuery, sqlConnection);
                        cmd6.Parameters.AddWithValue("@amount", amount);
                        cmd6.Parameters.AddWithValue("@units", unit);
                        cmd6.Parameters.AddWithValue("@price", price);
                        cmd6.Parameters.AddWithValue("@fund_name", fundCode.ToString());
                        cmd6.ExecuteNonQuery();
                        sqlConnection.Close();
                    }
                    
                    
                }
                else//insert a new hold fund
                {
                    string newFundQuery = "Insert INTO clientHoldFunds Values(@ave_price,@amount,@units,@fund_name,@account_id)";
                    newFundQuery+= "SELECT SCOPE_IDENTITY()";
                    sqlConnection.Open();
                    SqlCommand cmd5 = new SqlCommand(newFundQuery, sqlConnection);
                    cmd5.Parameters.AddWithValue("@ave_price", price);
                    cmd5.Parameters.AddWithValue("@amount", amount);
                    cmd5.Parameters.AddWithValue("@units",unit);
                    cmd5.Parameters.AddWithValue("@fund_name",fundCode);
                    cmd5.Parameters.AddWithValue("@account_id", accountId);
                    cmd5.ExecuteScalar();
                    sqlConnection.Close();

                }

                //create an entity to store client's gain/loss
                //buy funds won't create gain or loss only affect the average price, so here we only record sold orders
                if (unit<0 && isHold>=-unit)
                {
                    string getAveragePrice = "Select top(1)  * From clientHoldFunds where fund_name = @fund_code and account_id = " + accountId;
                    HoldFund holdFund= new HoldFund();                           
                    sqlConnection.Open();
                    SqlCommand cmdf = new SqlCommand(getAveragePrice, sqlConnection);
                    cmdf.Parameters.AddWithValue("@fund_code",fundCode.ToString());
                    SqlDataReader reader = cmdf.ExecuteReader();
                    while (reader.Read())
                    {

                        holdFund.Unit = Convert.ToInt32(reader["units"]);
                        holdFund.AveragePrice = Convert.ToDouble(reader["ave_price"]);
                        holdFund.Amount = Convert.ToDouble(reader["amount"]);
                        holdFund.FundCode = reader["fund_name"].ToString();

                       
                    }
                    sqlConnection.Close();

                    var profits = Convert.ToDouble( (holdFund.AveragePrice - price) * unit);
                    string profitQuery = "INSERT INTO margin VALUES(@margin_date,@fund_code,@profit,@curAvePrice,@markPrice,@curAmount,@holdUnits,@tradeUnits,@account_id)";
                    profitQuery += "SELECT SCOPE_IDENTITY()";
                    sqlConnection.Open();
                    SqlCommand cmdm = new SqlCommand(profitQuery, sqlConnection);
                    cmdm.Parameters.AddWithValue("@margin_date", today);
                    cmdm.Parameters.AddWithValue("@fund_code", fundCode.ToString());
                    cmdm.Parameters.AddWithValue("@profit", profits);
                    cmdm.Parameters.AddWithValue("@curAvePrice",holdFund.AveragePrice);
                    cmdm.Parameters.AddWithValue("@markPrice",price);
                    cmdm.Parameters.AddWithValue("@curAmount",holdFund.Amount);
                    cmdm.Parameters.AddWithValue("@holdUnits", holdFund.Unit);//after order the units of user holding at the moment
                    cmdm.Parameters.AddWithValue("@tradeUnits",unit);
                    cmdm.Parameters.AddWithValue("account_id",accountId);
                    cmdm.ExecuteNonQuery();
                    sqlConnection.Close(); 
                }
                


                //order excuted so update the order status to completed

                string statusQuery = "Update orders Set order_status = @status Where order_id = " + orderId;
                sqlConnection.Open();
                SqlCommand cmd7 = new SqlCommand(statusQuery, sqlConnection);
                cmd7.Parameters.AddWithValue("@status", "Completed");
                cmd7.ExecuteNonQuery();
                sqlConnection.Close();
                return Ok("Order Completed");
            }
            else
            {
                return Ok("No enough money");
                //todo order can be created but set the status to denied or discarded
            }

       

        }

        [HttpGet]
        public IHttpActionResult GetHoldingFunds(int accountId)
        {
            List<HoldFund>holdFunds = new List<HoldFund>();
            SqlConnection sqlConnection = new SqlConnection(connection);
            string query = "Select * From clientHoldFunds where account_id = " + accountId;
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            SqlDataReader reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                holdFunds.Add(new HoldFund(){
                    Unit =  Convert.ToInt32(reader["units"]),
                    AveragePrice = Convert.ToDouble(reader["ave_price"]),
                    Amount = Convert.ToDouble(reader["amount"]),
                    FundCode = reader["fund_name"].ToString()

                });
            }
            sqlConnection.Close();

            return Ok(holdFunds);
        }

        [HttpGet]
        public IHttpActionResult GetOrderHistory(int accountId)
        {
            List<Order> orders = new List<Order>();
            SqlConnection sqlConnection = new SqlConnection(connection);
            string query = "Select * From orders where account_id = " + accountId;
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            SqlDataReader reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                orders.Add(new Order()
                {
                    Units = Convert.ToInt32(reader["units"]),
                    Price = Convert.ToDouble(reader["price"]),
                    Amount = Convert.ToDouble(reader["amount"]),
                    FundCode = reader["fund_name"].ToString(),
                    OrderTime = (DateTime)reader["order_time"],
                    OrderStatus = reader["order_status"].ToString()

                });
            }
            sqlConnection.Close();


            return Ok(orders);
        }

        [HttpGet]
        public IHttpActionResult InvestmentPerformance(int accountId,string fundCode, string fromDate,string toDate)
        {
            SqlConnection sqlConnection = new SqlConnection(connection);
            string getAveragePrice = "Select top(1)  * From clientHoldFunds where fund_name = @fund_code and account_id = " + accountId;
            HoldFund holdFund = new HoldFund();
            sqlConnection.Open();
            SqlCommand cmdf = new SqlCommand(getAveragePrice, sqlConnection);
            cmdf.Parameters.AddWithValue("@fund_code", fundCode.ToString());
            SqlDataReader reader = cmdf.ExecuteReader();
            while (reader.Read())
            {

                holdFund.Unit = Convert.ToInt32(reader["units"]);
                holdFund.AveragePrice = Convert.ToDouble(reader["ave_price"]);
                holdFund.Amount = Convert.ToDouble(reader["amount"]);
                holdFund.FundCode = reader["fund_name"].ToString();


            }
            sqlConnection.Close();

            
            string query = "select Sum(profit) from margin where account_id = @accountId and fund_code = @fundCode and convert(date,margin_date) between @fromDate and @toDate ";
            
            sqlConnection.Open();
            SqlCommand sqlCommand = new SqlCommand(query,sqlConnection);
            sqlCommand.Parameters.AddWithValue("@accountId", accountId);
            sqlCommand.Parameters.AddWithValue("@fundCode", fundCode);
            sqlCommand.Parameters.AddWithValue("@fromDate", fromDate);
            sqlCommand.Parameters.AddWithValue("@toDate",toDate);
            var dbprofits = sqlCommand.ExecuteScalar();
            double profits=0.0;
            if (dbprofits.GetType().Name != "DBNull")
            {
               profits = Convert.ToDouble(dbprofits);
            }//null bug
            sqlConnection.Close();

            UpdateDailyPrice();
            bool IsWorkday = DateTime.Now.DayOfWeek != DayOfWeek.Sunday && DateTime.Now.DayOfWeek != DayOfWeek.Saturday;
            Double unrealized;
            if (IsWorkday)
            {
                string getPriceQuery = "Select price from securities where fund_code = @fundCode and Convert(date,price_date) = @toDate";
                sqlConnection.Open();
                SqlCommand getPriceCommand = new SqlCommand(getPriceQuery,sqlConnection);
                getPriceCommand.Parameters.AddWithValue("@fundCode",fundCode.ToString());
                getPriceCommand.Parameters.AddWithValue("@toDate", toDate);
                var price = Convert.ToDouble(getPriceCommand.ExecuteScalar());
                sqlConnection.Close();
                unrealized = holdFund.Unit * (holdFund.AveragePrice - price);

            }
            else
            {
                string priceDate;
                if(DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                {
                    priceDate = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
                }
                else
                {
                    priceDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                }
                string getPriceQuery = "Select price from securities where fund_code = @fundCode and Convert(date,price_date) = @toDate";
                sqlConnection.Open();
                SqlCommand getPriceCommand = new SqlCommand(getPriceQuery, sqlConnection);
                getPriceCommand.Parameters.AddWithValue("@fundCode", fundCode.ToString());
                getPriceCommand.Parameters.AddWithValue("@toDate", priceDate);
                var price = Convert.ToDouble(getPriceCommand.ExecuteScalar());
                sqlConnection.Close();
                unrealized = holdFund.Unit * (holdFund.AveragePrice - price);
            }
            //Total cost is the money spent during a certain period, but the gain percentage is the gain earned during this period devides the money spend on this fund all the time
            string costQuery = "Select Sum(amount) from orders where units>0 and account_id = @accountId and fund_name = @fundCode and convert(date,order_time) between @fromDate and @toDate";
            sqlConnection.Open();
            SqlCommand costCommand = new SqlCommand(costQuery, sqlConnection);
            costCommand.Parameters.AddWithValue("@accountId", accountId);
            costCommand.Parameters.AddWithValue("@fundCode", fundCode.ToString());
            costCommand.Parameters.AddWithValue("@fromDate",fromDate);
            costCommand.Parameters.AddWithValue("@toDate",toDate);
            var dbcost = costCommand.ExecuteScalar();
            double cost = 0.0;
            if(dbcost.GetType() == typeof(DBNull))
            {
                return Ok("Client hasn't bought this Fund");
            }
            //total cost during the period
            sqlConnection.Close();
            //get the first margin for geting the previous cost on the fund
            string marginQuery = "Select TOP(1) * from margin where account_id = @accountId and fund_code = @fundCode and margin_date >= @fromDate";
            sqlConnection.Open();
            SqlCommand marginCommand = new SqlCommand(marginQuery, sqlConnection);
            marginCommand.Parameters.AddWithValue("@accountId", accountId);
            marginCommand.Parameters.AddWithValue("@fundCode", fundCode);
            marginCommand.Parameters.AddWithValue("@fromDate", fromDate);
            SqlDataReader reader2 = marginCommand.ExecuteReader();
            Margin margin = new Margin();
            while (reader2.Read())
            {


                margin.HoldUnits = Convert.ToInt32(reader2["holdUnits"]);
                margin.TradeUnits = Convert.ToInt32(reader2["tradeUnits"]);
                margin.CurAmount = Convert.ToDouble(reader2["curAmount"]);
                margin.CurAvePrice = Convert.ToDouble(reader2["curAvePrice"]);
                
            }
            sqlConnection.Close();

            InvestPerformance performance = new InvestPerformance()
            {
                Gain = profits,
                UnrealizedGain = Math.Abs(unrealized),
                GainPercentage = profits/((margin.HoldUnits-margin.TradeUnits)*margin.CurAvePrice+cost),// GAIN = period gain devided by  previous costs + the new costs during this period
                Cost = cost,
                FundCode = fundCode,
                FromDate =Convert.ToDateTime(fromDate),
                ToDate =Convert.ToDateTime(toDate)
         
            };




            return Ok(performance);
        }
        public void UpdateDailyPrice()
        {
            
            //check whether today is a workday
            bool IsWorkday = DateTime.Now.DayOfWeek != DayOfWeek.Sunday && DateTime.Now.DayOfWeek != DayOfWeek.Saturday;
            if (IsWorkday)
            {
                DateTime today = DateTime.Now;
                var d = today.ToString("yyyy-MM-dd");
                SqlConnection sqlConnection = new SqlConnection(connection);
                string query1 = "Select Count(price) From securities Where convert(date,price_date) = @today ";
                SqlCommand command = new SqlCommand(query1, sqlConnection);
                command.Parameters.AddWithValue("@today",d);
                sqlConnection.Open();
                var isPriceExist = Convert.ToInt32(command.ExecuteScalar());
                sqlConnection.Close();
                //if today's price haven't created, create it
                if (isPriceExist<1)
                {
                    List<FundPrice> funds = new List<FundPrice>();
                    //get fundslist to see what kinds of funds we got. to prepare parameters for create new price list
                    string query = " Select distinct fund_code, fund_id  FROM securities";

                    sqlConnection.Open();
                    SqlCommand cmd = new SqlCommand(query, sqlConnection);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        funds.Add(new FundPrice()
                        {
                            PriceId = 0,
                            FundCode = reader["fund_code"].ToString(),
                            FundId = Convert.ToInt32(reader["fund_id"])
                        }); ;
                    }
                    sqlConnection.Close();
                    Random r = new Random();

                    //random a daily price for each fund in db
                    //create new entities in sercurities table
                    foreach (var f in funds)
                    {

                        var ranNum = Math.Round(r.NextDouble() * 200 + 5, 2);
                        f.PriceDate = today;
                        f.Price = ranNum;
                        string query2 = "Insert into securities Values(@fund_id,@price,@fund_code,@price_date)";
                        query2 += "SELECT SCOPE_IDENTITY()";
                        using (SqlConnection con = new SqlConnection(connection))
                        {
                            using (SqlCommand sqlCommand = new SqlCommand(query2))
                            {
                                sqlCommand.Parameters.AddWithValue("@fund_id", f.FundId);
                                sqlCommand.Parameters.AddWithValue("@price", ranNum);
                                sqlCommand.Parameters.AddWithValue("@fund_code", f.FundCode);
                                sqlCommand.Parameters.AddWithValue("@price_date", today);
                                sqlCommand.Connection = con;
                                con.Open();
                                f.PriceId = Convert.ToInt32(sqlCommand.ExecuteScalar());
                                con.Close();

                            }
                        }

                    }
                }
               

            }
        }


   
    }
}
