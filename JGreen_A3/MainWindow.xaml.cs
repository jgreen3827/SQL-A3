using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

/*
 *  File            : MainWindow.xaml.cs
 *  Project         : PROG3070 - Assignment 3
 *  Programmer      : Jordan Green
 *  First Version   : 2021-10-17
 *  Description     :
 *      This file will run the main components for assignment 3. The functionality will be all routed through the copy click
 *      event method. This will check if the source connection, destination connection, source db, table and destination db, table inputed by the user exist. If the source
 *      db or table do not exist an error will be printed. If the destination db does not exist an error will be printed. If all
 *      of these exist and the destination table does not it will be created in the destination table and the contents of the
 *      source table will be copied to it. If the destination table does exist then it will check if the column names and datatypes
 *      match the source table. If they do it will again copy if not then it will print an error to the screen.
 */

namespace JGreen_A3
{
    /*
     * Name     : UserInput
     * Purpose  : 
     *      This UserInput class will hold the contents of all the information input from the user. This makes it easier then having
     *      to pass 10 strings around to all the various methods.
     */
    public class UserInput
    {
        public string sourceDs;
        public string sourceUser;
        public string sourcePword;
        public string sourceDb;
        public string sourceTbl;
        public string destinationDs;
        public string destinationUser;
        public string destinationPword;
        public string destinationDb;
        public string destinationTbl;

        /*
         *  Function    : UserInput (Constructor)
         *  Description : 
         *      This will construct a UserInput object
         *  Parameters  :
         *      string srcDs        : Source Data Source
         *      string srcUser      : Source User Name
         *      string srcPword     : Source Password
         *      string srcDb        : Source Database
         *      string srcTbl       : Source Table
         *      string dstDs        : Destination Data Source
         *      string dstUser      : Destination User Name
         *      string dstPword     : Destination Password
         *      string dstnDb       : Destination Database
         *      string dstnTbl      : Destination Table
         *  Returns     :
         *      void
         */
        public UserInput(string srcDs, string srcUser, string srcPword, string srcDb, string srcTbl, string dstDs, string dstUser, string dstPword, string dstnDb, string dstnTbl)
        {
            sourceDs = srcDs;
            sourceUser = srcUser;
            sourcePword = srcPword;
            sourceDb = srcDb;
            sourceTbl = srcTbl;
            destinationDs = dstDs;
            destinationUser = dstUser;
            destinationPword = dstPword;
            destinationDb = dstnDb;
            destinationTbl = dstnTbl;
        }

    }

    /*
     * Name     : ConnectionCheck
     * Purpose  : 
     *      This UserInput class will hold the contents of whether the connection login worked or database exists.
     */
    public class ConnectionCheck
    {
        public bool failLogin;
        public bool dbExists;

        /*
         *  Function    : ConnectionCheck (Constructor)
         *  Description : 
         *      This will construct a ConnectionCheck object
         *  Parameters  :
         *      bool login  : if login worked
         *      bool db     : if database exists
         *  Returns     :
         *      void
         */
        public ConnectionCheck(bool login, bool db)
        {
            failLogin = login;
            dbExists = db;
        }
    }

    /*
     * Name     : TableInformation
     * Purpose  : 
     *      This Table Information class will hold the contents of a column in a table the column name, datatype, and max characters.
     */
    public class TableInformation
    {
        public string columnName;
        public string columnType;
        public string maxChar;

        /*
         *  Function    :   TableInformation (Constructor)
         *  Description :
         *      This will constuct a TableInformation objects.
         *  Parameters  :
         *      string name     :   name of table column
         *      string type     :   datatype of table column
         *      string max      :   the max number of character of datatype for table column
         *  Returns     :
         *      void
         */
        public TableInformation(string name, string type, string max)
        {
            columnName = name;
            columnType = type;
            maxChar = max;
        }
    }

    /*
     * Name     : MainWindow
     * Purpose  : 
     *      This is the MainWindow which will hold all of the fucntionality for the program/
     */
    public partial class MainWindow : Window
    {
        SqlConnection srcSqlConnection;
        SqlConnection destSqlConnection;

        /*
         *  Function    :   MainWindow
         *  Description :
         *      This will intialize the component for the program.
         *  Parameters  :
         *      none
         *  Returns     :
         *      void
         */
        public MainWindow()
        {
            InitializeComponent();
        }

        /*
         *  Function    :   CopyButton_Click
         *  Description :
         *      This is going to be called when the Copy Button is selected it will hold most of the functionality for the entire program.
         *  Parameters  :
         *      object sender       :   This is the object which sent the request
         *      RoutedEventArgs e   :   
         *  Returns     :
         *      void
         */
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            //reset error messages
            SourceDataSourceError.Content = "";
            SourceUserNameError.Content = "";
            SourcePwordError.Content = "";
            SourceDbError.Content = "";
            SourceTableError.Content = "";
            DestinationDataSourceError.Content = "";
            DestinationUsernameError.Content = "";
            DestinationPwordError.Content = "";
            DestinationDbError.Content = "";
            DestinationTableError.Content = "";

            // clear data grids
            SourceDG.ItemsSource = null;
            SourceDG.Items.Refresh();
            DestinationDG.ItemsSource = null;
            DestinationDG.Items.Refresh();

            //get all inputs
            UserInput userInputs = new UserInput(
                SourceDataSourceInput.Text,
                SourceUsernameInput.Text,
                SourcePwordInput.Text,
                SourceDbInput.Text, 
                SourceTableInput.Text, 
                DestinationDataSourceInput.Text,
                DestinationUsernameInput.Text,
                DestinationPwordInput.Text,
                DestinationDbInput.Text, 
                DestinationTableInput.Text
            );

            //check if all inputs were filled in
            if(VerifyInputsFilled(userInputs) == false)
            {
                return;
            }

            //set up SQL Connections
            string srcSqlConnectionString = "Data Source=" + userInputs.sourceDs + ";Initial Catalog=" + userInputs.sourceDb + ";User ID=" + userInputs.sourceUser + ";Password=" + userInputs.sourcePword + ";";
            string dstSqlConnectionString = "Data Source=" + userInputs.destinationDs + ";Initial Catalog=" + userInputs.destinationDb + ";User ID=" + userInputs.destinationUser + ";Password=" + userInputs.destinationPword + ";";
            srcSqlConnection = new SqlConnection(srcSqlConnectionString);
            destSqlConnection = new SqlConnection(dstSqlConnectionString);

            //check connection to ensure Data source, username, and password are correct and database exists
            ConnectionCheck sourceConnection = CheckConnection(userInputs.sourceDs, userInputs.sourceUser, userInputs.sourcePword, userInputs.sourceDb);
            ConnectionCheck destinationConnection = CheckConnection(userInputs.destinationDs, userInputs.destinationUser, userInputs.destinationPword, userInputs.destinationDb);

            //set the error messages needed based on what failed
            if(sourceConnection.failLogin == false || destinationConnection.failLogin == false || sourceConnection.dbExists == false || destinationConnection.dbExists == false)
            {
                if(sourceConnection.failLogin == false)
                {
                    SourceDataSourceError.Content = "Error with connecting/logging in to the Data Source..";
                }

                if(destinationConnection.failLogin == false)
                {
                    DestinationDataSourceError.Content = "Error with connecting/logging in to the Data Source..";
                }

                if(sourceConnection.dbExists == false)
                {
                    SourceDbError.Content = userInputs.sourceDb + " does not exist, enter a correct database..";
                }

                if(destinationConnection.dbExists == false)
                {
                    DestinationDbError.Content = userInputs.destinationDb + " does not exist, enter a correct database..";
                }

                return;
            }

            //check if tables exist
            bool sourceTable = CheckTableExists(userInputs.sourceTbl, srcSqlConnection);
            bool destinationTable = CheckTableExists(userInputs.destinationTbl, destSqlConnection);
            
            if (sourceTable == false)
            {
                //if the source table does not exist send error to user
                SourceTableError.Content = userInputs.sourceTbl + " does not exist, enter a correct table..";
                return;
            }

            //fill source data grid
            FillDataGrid(userInputs.sourceDb, userInputs.sourceTbl, SourceDG, srcSqlConnection);
 
            if (destinationTable == false)
            {
                //if destination table does not exist then create the table and copy all the records from source to destination
                if (CreateDestinationTable(userInputs, destSqlConnection) == false)
                {
                    return;
                }

                if(SourceToDestination(userInputs) == false)
                {
                    return;
                }
            }
            else
            {
                //both db and table exist
                DestinationDbError.Content = "";
                DestinationTableError.Content = "";

                //check if the table column names and datatypes match the source column names and datatypes
                bool tablesMatch = CompareTables(userInputs);

                if (tablesMatch == true)
                {
                    //if tables match then copy from source to destination
                    if(SourceToDestination(userInputs) == false)
                    {
                        return;
                    }
                }
                else
                {
                    //if destination table doesn't match source table column names, and datatypes send error to user
                    DestinationTableError.Content = userInputs.destinationTbl + " columns and/or datatypes do not match the source table, please enter a correct table..";
                    return;
                }
            }

            //fill destination data grid
            FillDataGrid(userInputs.destinationDb, userInputs.destinationTbl, DestinationDG, destSqlConnection);
        }

        /*
         *  Function    :   VerifyInputsFilled
         *  Description :
         *      This will check each of the input fields and ensure that they have some information. Any input that does not have any value
         *      it will set the error message and return false;
         *  Parameters  :
         *      UserInput input :   This is an object of all the inputs from the UI.
         *  Returns     :
         *      true  :     This will return if all inputs have been filled.
         *      false :     This will return if any of the inputs are empty.
         */
        public bool VerifyInputsFilled(UserInput input)
        {
            bool allFilled = true;

            if(input.sourceDs == "")
            {
                SourceDataSourceError.Content = "This field must be filled out..";
                allFilled = false;
            }

            if(input.sourceUser == "")
            {
                SourceUserNameError.Content = "This field must be filled out..";
                allFilled = false;
            }

            if(input.sourcePword == "")
            {
                SourcePwordError.Content = "This field must be filled out..";
                allFilled = false;
            }

            if (input.sourceDb == "")
            {
                SourceDbError.Content = "This field must be filled out..";
                allFilled = false;
            }

            if (input.sourceTbl == "")
            {
                SourceTableError.Content = "This field must be filled out..";
                allFilled = false;
            }

            if(input.destinationDs == "")
            {
                DestinationDataSourceError.Content = "This field must be filled out..";
                allFilled = false;
            }

            if(input.destinationUser == "")
            {
                DestinationUsernameError.Content = "This field must be filled out..";
                allFilled = false;
            }

            if(input.destinationPword == "")
            {
                DestinationPwordError.Content = "This field must be filled out..";
                allFilled = false;
            }

            if (input.destinationDb == "")
            {
                DestinationDbError.Content = "This field must be filled out..";
                allFilled = false;
            }

            if (input.destinationTbl == "")
            {
                DestinationTableError.Content = "This field must be filled out..";
                allFilled = false;
            }

            return allFilled;
        }

        /*
         *  Function    :   CheckConnection
         *  Description :
         *      This will check if the connection string being generated from the parameters can connect. It will determine if the datasource
         *      exists, and if the username and password can log into that data source. It will then check if the db exists and will return 
         *      a boolean whether or not it does
         *  Parameters  :
         *      string ds           : This is the data source to be tested
         *      string user         : This is the username to be tested
         *      string pword        : This is the password to be tested
         *      string databaseName : This is the database name to be tested
         *  Returns     :
         *      ConnectionCheck : This will return a Connection check object which returns a boolean for if the login failed, and a boolean if
         *                        the database exists or not
         */
        private ConnectionCheck CheckConnection(string ds, string user, string pword, string databaseName)
        {
            string srcSqlConnectionString = "Data Source=" + ds + ";Initial Catalog=master;User ID=" + user + ";Password=" + pword + ";";
            string dbCmdText = "SELECT COUNT(*) FROM master.dbo.sysdatabases WHERE name=@database";
            SqlConnection sqlConnection =  new SqlConnection(srcSqlConnectionString);
            bool databaseExists = false;
            try
            {
                sqlConnection.Open();
                try
                {
                    //check if the database exists
                    SqlCommand dbCmd = new SqlCommand(dbCmdText, sqlConnection);
                    dbCmd.Parameters.Add("@database", System.Data.SqlDbType.NVarChar).Value = databaseName;
                    databaseExists = Convert.ToInt32(dbCmd.ExecuteScalar()) == 1;

                    if(databaseExists == false)
                    {
                        //the database does not exist
                        return new ConnectionCheck(true, false);
                    }
                }
                catch (Exception ex)
                {
                    //if there was an error with checking for the db
                    MessageBox.Show("SQL ERROR -- \n" + ex.Message);
                    return new ConnectionCheck(false, true);
                }
            }
            catch (Exception ex)
            {
                //if the login fails or data source doesnt exist
                MessageBox.Show("SQL ERROR -- \n" + ex.Message);
                return new ConnectionCheck(false, true);
            }

            //if everything worked correctly
            return new ConnectionCheck(true, true);
        }

        /*
         *  Function    : CheckTableExists
         *  Description :
         *      This will check if the incoming table exists in the sql connection and the database being passed in the parameter.
         *  Parameters  :
         *      string tableName            : This is the table that will be checking if it exists
         *      SqlConnection sqlConnection : This the connection to be used to check for the table
         *  Returns     :
         *      bool : This will return whether the table exists or not.
         */
        private bool CheckTableExists(string tableName, SqlConnection sqlConnection)
        {
            string tableCmdText = "IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = N'" + tableName + "') SELECT 1 ELSE SELECT 0";
            bool tableExists = false;
            try
            {
                sqlConnection.Open();

                SqlCommand tblCmd = new SqlCommand(tableCmdText, sqlConnection);
                tableExists = Convert.ToInt32(tblCmd.ExecuteScalar()) == 1;

                sqlConnection.Close();
            } 
            catch (Exception ex)
            {
                MessageBox.Show("SQL ERROR -- \n" + ex.Message);
            }

            return tableExists;

        }

        /*
         *  Function    : CompareTables
         *  Description :
         *      This will get the information from each of the tables column names and datatypes. It will then call the function that will
         *      compare the list of source and destination.
         *  Parameters  :
         *      UserInput userInput : This is all of the inputs from the user.
         *  Returns     :
         *      bool : This will return the whether the source and destination tables match.
         */
        private bool CompareTables(UserInput userInput)
        {
            string sourceCmdText = "SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + userInput.sourceTbl + "';";
            string destCmdText = "SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + userInput.destinationTbl + "';";

            //get information from source and destination tables
            List<TableInformation> srcTableInfo = GetTableInfo(sourceCmdText, srcSqlConnection);
            List<TableInformation> dstnTableInfo = GetTableInfo(destCmdText, destSqlConnection);

            //check if they are the same
            bool isEqual = CompareLists(srcTableInfo, dstnTableInfo);
            MessageBox.Show(isEqual.ToString());

            return isEqual;
        }

        /*
         *  Function    : CompareLists
         *  Description :
         *      This will compare the source list against the destination list
         *  Parameters  :
         *      List<TableInformation> src  : List of all the source table info
         *      List<TableInformation> dest : List of all the destination table info
         *  Returns     :
         *      bool: returns whether the two lists are the same or not
         */
        private bool CompareLists(List<TableInformation> src, List<TableInformation> dest)
        {
            //check against source
            for (int i = 0; i < src.Count(); i++)
            {
                for (int j = 0; j < dest.Count(); j++)
                {
                    if ((src[i].columnName == dest[j].columnName) && (src[i].columnType == dest[j].columnType) && (src[i].maxChar == dest[j].maxChar))
                    {
                        break;
                    }

                    if (j + 1 == dest.Count)
                    {
                        //error src column did not have a match in the destination column
                        return false;
                    }
                }
            }

            //check against destination
            for (int i = 0; i < dest.Count(); i++)
            {
                for (int j = 0; j < src.Count(); j++)
                {
                    if ((src[j].columnName == dest[i].columnName) && (src[j].columnType == dest[i].columnType) && (src[j].maxChar == dest[i].maxChar))
                    {
                        break;
                    }

                    if (j + 1 == dest.Count)
                    {
                        //error destination column did not have a match in the source column
                        return false;
                    }
                }
            }

            return true;
        }

        /*
         *  Function    : GetTableInfo
         *  Description :
         *      This will get all the info for a table Column name, datatype, and max character and return a list
         *  Parameters  :
         *      string cmdText              : This is the command text to be used in the sql call
         *      SqlConnection sqlConnection : This is the SQLConnection to be used in the sql call
         *  Returns     :
         *      List<TableInformation>: This will return the list of every column in the table (column name, datatype, max characters)
         */
        private List<TableInformation> GetTableInfo(string cmdText, SqlConnection sqlConnection)
        {
            SqlDataReader dataReader;
            List<TableInformation> tableInfo = new List<TableInformation>();

            try
            {
                sqlConnection.Open();

                SqlCommand tableCmd = new SqlCommand(cmdText, sqlConnection);
                dataReader = tableCmd.ExecuteReader();
                while (dataReader.Read())
                {
                    string result = ReadSingleRow((IDataRecord)dataReader);
                    string[] resultArray = result.Split('-');
                    tableInfo.Add(new TableInformation(resultArray[0], resultArray[1], resultArray[2]));
                }
                dataReader.Close();
                tableCmd.Dispose();

                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("SQL ERROR -- \n" + ex.Message);
            }
   
            return tableInfo;
        }

        /*
         *  Function    : CreateDestinationTable
         *  Description :
         *      This will create the destination table to table
         *  Parameters  :
         *      UserInput userInput         : This holds all of the user inputs
         *      SqlConnection sqlConnection : This is the sql connection to be used to make the table
         *  Returns     :
         *      bool : This will return whether or not the create table worked successfully
         */
        private bool CreateDestinationTable(UserInput userInput, SqlConnection sqlConnection)
        {
            string sourceCmdText = "SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'" + userInput.sourceTbl + "';";
            List<TableInformation> srcTableInfo = GetTableInfo(sourceCmdText, srcSqlConnection);

            string createDestCmdText = "CREATE TABLE " + userInput.destinationTbl + " (";

            for (int i = 0; i < srcTableInfo.Count(); i++)
            {
                if((i + 1) == srcTableInfo.Count())
                {
                    if (srcTableInfo[i].maxChar == "")
                    {
                        createDestCmdText = createDestCmdText + " " + srcTableInfo[i].columnName + " " + srcTableInfo[i].columnType + ");";
                    }
                    else
                    {
                        createDestCmdText = createDestCmdText + " " + srcTableInfo[i].columnName + " " + srcTableInfo[i].columnType + "(" + srcTableInfo[i].maxChar + ") );";
                    }
                } else
                {
                    if(srcTableInfo[i].maxChar == "")
                    {
                        createDestCmdText = createDestCmdText + " " + srcTableInfo[i].columnName + " " + srcTableInfo[i].columnType + ",";
                    } else
                    {
                        createDestCmdText = createDestCmdText + " " + srcTableInfo[i].columnName + " " + srcTableInfo[i].columnType + "(" + srcTableInfo[i].maxChar + "),";
                    }
                    
                }
            }

            try
            {
                sqlConnection.Open();
                SqlCommand createCmd = new SqlCommand(createDestCmdText, sqlConnection);
                createCmd.ExecuteNonQuery();

                createCmd.Dispose();
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("SQL ERROR -- \n" + ex.Message);
                return false;
            }

            return true;
         }

        /*
         *  Function    : SourceToDestination
         *  Description :
         *      This will copy all of the data from teh source table to the destination table
         *  Parameters  :
         *      UserInput userInput : This is all of the input from the user
         *  Returns     :
         *      bool : This will determine whether the copy has worked correctly
         */
        private bool SourceToDestination(UserInput userInput)
        {
            SqlCommand srcCommand = null;
            string srcCommandTxt = "SELECT * FROM " + userInput.sourceTbl + ";";

            srcSqlConnection.Open();
            srcCommand = new SqlCommand(srcCommandTxt, srcSqlConnection);
            SqlDataReader reader = srcCommand.ExecuteReader();

            destSqlConnection.Open();
            SqlTransaction transaction = destSqlConnection.BeginTransaction();
            SqlBulkCopy bulkCopy = new SqlBulkCopy(destSqlConnection, SqlBulkCopyOptions.KeepIdentity, transaction);
            bulkCopy.DestinationTableName = userInput.destinationTbl;
            try
            {
                // Write from the source to the destination.
                bulkCopy.WriteToServer(reader);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                try
                {
                    transaction.Rollback();
                    destSqlConnection.Close();
                    reader.Close();
                    srcSqlConnection.Close();
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("Rollback Exception Type -- \n" + ex2.Message);
                    destSqlConnection.Close();
                    reader.Close();
                    srcSqlConnection.Close();
                }
                return false;
            }
            finally
            {
                destSqlConnection.Close();
                reader.Close();
                srcSqlConnection.Close();
            }
            return true;
        }

        /*
         *  Function    : ReadSingleRow
         *  Description :
         *      This will read from the dataRecord and return a formatted string.
         *  Parameters  :
         *      IDataRecord dataRecord : This is a data record that was read from the SQL table
         *  Returns     :
         *      String : This will return a formatted string of the seprated values that came from the data record
         */
        private static String ReadSingleRow(IDataRecord dataRecord)
        {
            return String.Format("{0}-{1}-{2}", dataRecord[0], dataRecord[1], dataRecord[2]);
        }

        /*
         *  Function    : FillDataGrid
         *  Description :
         *      This will populate the Data Grid withh all the information from the specified database
         *  Parameters  :
         *      string database             : This is the database that the data will come from 
         *      string table                : This is the table that the data will come from 
         *      DataGrid dataGrid           : This is the datagrid that we will populate with the data
         *      SqlConnection sqlConnection : This is the connection used to read the data
         *  Returns     :
         *      void
         */
        private void FillDataGrid(string database, string table, DataGrid dataGrid, SqlConnection sqlConnection)
        {
            string daCmdText = "SELECT * FROM " + table;
            SqlCommand displayCommand = new SqlCommand(daCmdText, sqlConnection);
            SqlDataAdapter dataAdapter = new SqlDataAdapter(displayCommand);
            DataTable dataTable = new DataTable(table);
            dataAdapter.Fill(dataTable);
            dataGrid.ItemsSource = dataTable.DefaultView;
        }
    }
}
