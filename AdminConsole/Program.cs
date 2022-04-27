using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace AdminConsole
{
    internal class Program
    {


        static void Main(string[] args)
        {
            Console.WriteLine("1. Read table and export to csv file \n2. Get report and save to csv");
            int choice;
            Console.WriteLine("Enter the number of the opition: ");
            choice = Convert.ToInt32(Console.ReadLine());
            switch (choice)
            {
                case 1:
                    OutputCsv();
                    break;
                case 2:
                    CallFundSys();
                    break;
                default:
                    Console.WriteLine("Please enter a valid input");
                    break;

            }


        }

        //export csv
        static void OutputCsv()
        {
            string datetime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string LogFolder = @"C:\FundDemo\";
            try
            {
                string TableName;
                Console.WriteLine("Enter the table name that you want to read: ");
                Console.WriteLine("Table Lists: \n cashEntry\n clientHoldFunds\n instrument\n margin\n orders\n SECClientAccount\n sercurities\n tblClient\n");
                TableName = Console.ReadLine();

                string constr = ConfigurationManager.ConnectionStrings["MyConnection"].ConnectionString;
                string FileNamePart = TableName;//Datetime will be added to it
                string DestinationFolder = @"C:\FundDemo\";

                string FileDelimiter = ","; //You can provide comma or pipe or whatever you like
                string FileExtension = ".csv"; //Provide the extension you like such as .txt or .csv

                SqlConnection con = new SqlConnection(constr);
                //Create Connection to SQL Server in which you like to load files


                //Read data from table or view to data table
                string query = "Select * From " + TableName;
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                DataTable d_table = new DataTable();
                d_table.Load(cmd.ExecuteReader());
                con.Close();

                //Prepare the file path 
                string FileFullPath = DestinationFolder + "\\" + FileNamePart + "_" + datetime + FileExtension;

                StreamWriter sw = null;
                sw = new StreamWriter(FileFullPath, false);

                // Write the Header Row to File
                int ColumnCount = d_table.Columns.Count;
                for (int ic = 0; ic < ColumnCount; ic++)
                {
                    sw.Write(d_table.Columns[ic]);
                    if (ic < ColumnCount - 1)
                    {
                        sw.Write(FileDelimiter);
                    }
                }
                sw.Write(sw.NewLine);

                // Write All Rows to the File
                foreach (DataRow dr in d_table.Rows)
                {
                    for (int ir = 0; ir < ColumnCount; ir++)
                    {
                        if (!Convert.IsDBNull(dr[ir]))
                        {
                            sw.Write(dr[ir].ToString());
                        }
                        if (ir < ColumnCount - 1)
                        {
                            sw.Write(FileDelimiter);
                        }
                    }
                    sw.Write(sw.NewLine);

                }

                sw.Close();
            }
            catch (Exception ex)
            {
                // Create Log File for Errors
                using (StreamWriter sw = File.CreateText(LogFolder
                    + "\\" + "ErrorLog_" + datetime + ".log"))
                {
                    sw.WriteLine(ex.ToString());

                }
                Console.Write(ex.ToString());
            }
        }

        static void CallFundSys()
        {
            HttpClient client = new HttpClient();
            string APIUrl = "https://localhost:44394/api/operation/getmarginreport";

            client.BaseAddress = new Uri(APIUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var responseTask = client.GetAsync(APIUrl);
            responseTask.Wait();

            if (responseTask.IsCompleted)
            {
                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var messageTask = result.Content.ReadAsStringAsync();
                    messageTask.Wait();
                    Console.WriteLine(messageTask.Result);

                    try
                    {
                        DataTable d_table = JsonConvert.DeserializeObject<DataTable>(messageTask.Result.ToString());
                        string DestinationFolder = @"C:\FundDemo\";
                        string datetime = DateTime.Now.ToString("yyyyMMddHHmmss");
                        string FileDelimiter = ","; //You can provide comma or pipe or whatever you like
                        string FileExtension = ".csv"; //Provide the extension you like such as .txt or .csv
                        string FileFullPath = DestinationFolder + "\\" + "Report" + "_" + datetime + FileExtension;

                        StreamWriter sw = null;
                        sw = new StreamWriter(FileFullPath, false);

                        // Write the Header Row to File
                        int ColumnCount = d_table.Columns.Count;
                        for (int ic = 0; ic < ColumnCount; ic++)
                        {
                            sw.Write(d_table.Columns[ic]);
                            if (ic < ColumnCount - 1)
                            {
                                sw.Write(FileDelimiter);
                            }
                        }
                        sw.Write(sw.NewLine);

                        // Write All Rows to the File
                        foreach (DataRow dr in d_table.Rows)
                        {
                            for (int ir = 0; ir < ColumnCount; ir++)
                            {
                                if (!Convert.IsDBNull(dr[ir]))
                                {
                                    sw.Write(dr[ir].ToString());
                                }
                                if (ir < ColumnCount - 1)
                                {
                                    sw.Write(FileDelimiter);
                                }
                            }
                            sw.Write(sw.NewLine);

                        }

                        sw.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                    }
                    Console.ReadLine();

                }

            }
            else
            {
                Console.WriteLine("Internal server Error");
            }
            Console.WriteLine("Complete");
        }
    }
}
