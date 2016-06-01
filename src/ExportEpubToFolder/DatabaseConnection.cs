using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using System.Configuration;

namespace ExportEpubToFolder
{
    class DatabaseConnection
    {
        private MySqlConnection connection;
        private string server;
        //private string database;
        private string uid;
        private string password;
        //server = ConfigurationSettings.AppSettings["databaseserver"];
        public DatabaseConnection()
        {
            Initialize();
        }
        //Initialize values
        private void Initialize()
        {
            server = ConfigurationSettings.AppSettings["databaseserver"];
            //database = "dbproduction";
            uid = ConfigurationSettings.AppSettings["databaseusername"];
            password = ConfigurationSettings.AppSettings["databasepassword"];
            string connectionString;
            //connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            connectionString = "Server=" + server + ";userid=" + uid + ";password=" + password +";";
            //string connectionString = "Server=" + JobCloseEmail.Program.connectionString + ";userid=root;password=root;";
            connection = new MySqlConnection(connectionString);
            MySqlCommand commdEx = new MySqlCommand();
            commdEx.Connection = connection;
            commdEx.CommandTimeout = 200;
        }
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        //MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        //MessageBox.Show("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }
        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                //MessageBox.Show(ex.Message);
                return false;
            }
        }
        public DataTable getData(string queryString)
        {
            //string connectionString = "Server=" + JobCloseEmail.Program.connectionString + ";userid=root;password=root;";
            DataTable dtDetails = new DataTable();
            if (this.OpenConnection() == true)
            {
                MySqlDataAdapter da = new MySqlDataAdapter(queryString, connection);
                da.Fill(dtDetails);
                this.CloseConnection();
                return dtDetails;
            }
            else
                return dtDetails;

        }
        public string getScalarData(string queryString)
        {
            //string connectionString = "Server=" + JobCloseEmail.Program.connectionString + ";userid=root;password=root;";
            //DataTable dtDetails = new DataTable();
            string details="";
            if (this.OpenConnection() == true)
            {
                MySqlDataAdapter da = new MySqlDataAdapter(queryString, connection);
                //da.Fill(dtDetails);
                MySqlCommand commd = new MySqlCommand(queryString);
                details = commd.ExecuteScalar().ToString(); 
                this.CloseConnection();
                return details;
            }
            else
                return details;

        }
        public void Insert(string QueryString)
        {
            //open connection
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(QueryString, connection);

                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        public void Update(string QueryString)
        {
            //open connection
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(QueryString, connection);

                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }
    }
}
