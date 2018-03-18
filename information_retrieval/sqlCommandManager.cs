using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace information_retrieval
{
    class sqlCommandManager
    {
        public static string connString = "Server=localhost;Port=3306;Database=documents;Uid=root;password=1307;";
        public static MySqlConnection conn = new MySqlConnection(connString);

        //create connection
        public static MySqlConnection createConnection()
        {
            MySqlCommand command = conn.CreateCommand();

            try
            {
                conn.Open();
            }
            catch { }
            return conn;
        }

        //close connection
        public static void closeConnection()
        {
            try
            {
                conn.Close();
            }
            catch { }
        }

        public static void createDBdata()
        {
            createTable("word_index (word_name varchar(20),doc_index int, PRIMARY KEY(word_name,doc_index));");
        }

        //create table
        public static void createTable(string data)
        {
            try
            {
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "CREATE TABLE if not exists " + data;
                command.ExecuteNonQuery();      // running insert command
            }
            catch { }
        }

        //insert data to table
        public static void insertData(string data)
        {
            try
            {
                MySqlCommand command = conn.CreateCommand();
                command.CommandText = "Insert INTO " + data;
                command.ExecuteNonQuery();      // running insert command
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }
        }

        public static DataTable selectCommand(string data)
        {
            MySqlCommand command = new MySqlCommand(data, conn);
            DataTable dt = new DataTable();
            dt.Load(command.ExecuteReader());
            dt.Columns.Add("Summary");
            foreach (DataRow row in dt.Rows)
            {
                List<string> a = new List<string>();
                string index = row["doc_index"].ToString();
                a = Robots.addSummary(index);
                row["Summary"] = a[0];
            }
            return dt;
        }
    }
}
