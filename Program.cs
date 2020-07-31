using System;
using System.Data.SQLite;
using System.Net;
using System.IO;
using System.Timers;

namespace Valuta
{
    class Program
    {
        static SQLiteConnection conn = new SQLiteConnection("Data Source=new_database.db;Version=3;");

        static int lastIndex=0;
        static void Main(string[] args)
        {
            try
            {
                conn.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                Console.WriteLine(e.StackTrace);
            }
            
            var timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(TimerStep);
            timer.Interval = 1000*60*60*24;
            timer.Start();

            Console.WriteLine("Нажмите любую клавишу для завершения");
            Console.ReadLine();
            
            conn.Close();
        }
        public static void TimerStep(object source, ElapsedEventArgs e)
        {
            Load((DateTime.Today).ToString("d"));
        }
        public static void Load(string currentDate)
        {
            var strURL = "http://www.cbr.ru/scripts/XML_daily.asp?date_req=" + currentDate;
            var objWebRequest = (HttpWebRequest)WebRequest.Create(strURL);
            objWebRequest.Method = "GET";
            var objWebResponse = (HttpWebResponse)objWebRequest.GetResponse();
            var streamReader = new StreamReader(objWebResponse.GetResponseStream());
            var strHTML = streamReader.ReadToEnd();
            streamReader.Close();
            objWebResponse.Close();
            objWebRequest.Abort();
            
            var ID= FindText(strHTML, "<Valute ID=\"", "\">");
            var Value = FindText(strHTML, @"<Value>", @"</Value>");
            while(ID != String.Empty)
            {
                var sql = "SELECT * FROM ValCurs WHERE (ID_val='" + ID + "' AND Date_req='" + currentDate + "')";
                var command = new SQLiteCommand(sql, conn);
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var sql3 = "UPDATE ValCurs SET Value='" + Value + "' WHERE(ID_val='" + ID + "' AND Date_req='" + currentDate + "')";
                    var command3 = new SQLiteCommand(sql3, conn);
                    command3.ExecuteNonQuery();
                }
                else
                {
                    var sql2 = "INSERT INTO ValCurs VALUES('"+ ID + "','" + Value + "','"+currentDate+"')";
                    var command2 = new SQLiteCommand(sql2, conn);
                    command2.ExecuteNonQuery();
                }
                ID = FindText(strHTML, "<Valute ID=\"", "\">");
                Value = FindText(strHTML, @"<Value>", @"</Value>");
            }
        }
        public static String FindText(string source, string prefix, string suffix)
        {
            var prefixPosition = source.IndexOf(prefix,lastIndex, StringComparison.OrdinalIgnoreCase);
            var suffixPosition = source.IndexOf(suffix, prefixPosition + prefix.Length, StringComparison.OrdinalIgnoreCase);

            if ((prefixPosition >= 0) && (suffixPosition >= 0) && (suffixPosition > prefixPosition) && ((prefixPosition + prefix.Length) <= suffixPosition))
            {
                lastIndex = prefixPosition + prefix.Length + suffixPosition - prefixPosition - prefix.Length;
                return source.Substring(prefixPosition + prefix.Length, suffixPosition - prefixPosition - prefix.Length);
            }
            else
            {
                return String.Empty;
            }
        }
        
        public static string getValutaInDate(string valuta, string date)
        {
            var sql = "SELECT Value FROM ValCurs JOIN Valuta ON (ValCurs.ID_val=Valuta.ID) WHERE Name='" + valuta +
                "' AND Date_req='" + date + "'";
            var command = new SQLiteCommand(sql, conn);
            var reader = command.ExecuteReader();
            reader.Read();
            return (reader["Value"]).ToString();
        }
    }
}
