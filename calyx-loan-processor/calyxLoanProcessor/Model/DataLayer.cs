using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Security.Cryptography;
using System.Net;
using System.Globalization;

public sealed class DataLayer
{
    //Using Singleton Pattern for Having single object of This Class For The Entire Solution..
    static readonly DataLayer _dl = new DataLayer();

    DataLayer() { }

    public static DataLayer dl
    {
        get
        {
            return _dl;
        }
    }
    //-------------------------------------Singleton Pattern End--------------

    DataUtility objDut = new DataUtility();
    //PopMsg popMsg = new PopMsg();
    FileIO objIO = new FileIO();
    //CreateLog objLog = new CreateLog();

    public void InsertLogInDB(string DML_Type, string TableName, string TableID)
    {
        //objLog.InsertLog(DML_Type, TableName, TableID);
    }

    public void ExportLogInText(DataTable old, DataTable updated, string TableName, string TableID)
    {
        //objLog.ExportUpdateLog(old, updated, TableName, TableID);
    }

    //Create Every Procedure with ID when ID = 0 It will Insert and if ID will somthing else it will Update That ID Row
    public bool SaveDataByProcedure(string table, string Procedure, params SqlParameter[] par)
    {
        int i = objDut.ExecSql(Procedure, par);
        InsertLogInDB("Insert", table, getMaxID(table));//Insert Executed Log Detail
        if (i > 0)
            return true;
        else
            return false;
    }

    public bool SaveDataByParameter(string table, params SqlParameter[] par)
    {
        string col = "";
        string val = "";
        foreach (SqlParameter p in par)
        {
            col += p.ParameterName.Remove(0, 1) + ",";
            val += p.ParameterName + ",";
        }
        col = col.Remove(col.Length - 1);
        val = val.Remove(val.Length - 1);
        //Main Query
        string query = "Insert into " + table + " (" + col + ") values (" + val + ") ";
        bool res = ExecuteSQL(query, par);
        InsertLogInDB("Insert", table, getMaxID(table));//Insert Executed Log Detail
        return res;
    }

    public bool SaveDataByParameter(string table, out int InsertedID, params SqlParameter[] par)
    {
        string col = "";
        string val = "";
        foreach (SqlParameter p in par)
        {
            col += p.ParameterName.Remove(0, 1) + ",";
            val += p.ParameterName + ",";
        }
        col = col.Remove(col.Length - 1);
        val = val.Remove(val.Length - 1);
        //Main Query
        string query = "Insert into " + table + " (" + col + ") values (" + val + ") select @ID = @@IDENTITY";
        SqlParameter par_OutID = par_out("@ID", SqlDbType.Int);
        List<SqlParameter> parameter = new List<SqlParameter>();
        parameter.AddRange(par.ToList<SqlParameter>());
        parameter.Add(par_OutID);
        bool res = ExecuteSQL(query, parameter.ToArray());
        InsertedID = int.Parse(par_OutID.Value.ToString());
        InsertLogInDB("Insert", table, getMaxID(table));//Insert Executed Log Detail
        return res;
    }

    public bool SaveDataByParameter(string table, bool withoutLOG, params SqlParameter[] par)
    {
        string col = "";
        string val = "";
        foreach (SqlParameter p in par)
        {
            col += p.ParameterName.Remove(0, 1) + ",";
            val += p.ParameterName + ",";
        }
        col = col.Remove(col.Length - 1);
        val = val.Remove(val.Length - 1);
        //Main Query
        string query = "Insert into " + table + " (" + col + ") values (" + val + ") ";
        bool res = ExecuteSQL(query, par);
        if (withoutLOG)
            InsertLogInDB("Insert", table, getMaxID(table));//Insert Executed Log Detail
        return res;
    }

    public bool SaveDataByQuery(string[,] column_values, string table)
    {
        bool res = objDut.insert(column_values, table);
        InsertLogInDB("Insert", table, getMaxID(table));//Insert Executed Log Detail
        return res;
    }

    public bool UpdateDataByProcedure(string ID, string table, string Procedure, params SqlParameter[] par)
    {
        DataTable old = objDut.GetTable("select * from " + table + " where ID=" + ID);
        int i = objDut.ExecSql(Procedure, par);
        if (i > 0)
        {
            DataTable updated = objDut.GetTable("select * from " + table + " where ID=" + ID);
            InsertLogInDB("Update", table, ID);//Insert Executed Log Detail
            ExportLogInText(old, updated, table, ID); //Exported Log Data to .dat file see function for more detail
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool UpdateDataByParameter(string table, string ID, params SqlParameter[] par)
    {
        string setv = "set ";
        foreach (SqlParameter p in par)
        {
            //column=@column
            setv += p.ParameterName.Remove(0, 1) + "=" + p.ParameterName + ",";
        }
        setv = setv.Remove(setv.Length - 1);
        //Main Query
        string query = "Update " + table + " " + setv + " where ID=" + ID;

        //UpdateLOg
        DataTable old = objDut.GetTable("select * from " + table + " where ID=" + ID);
        bool res = ExecuteSQL(query, par);
        if (res)
        {
            DataTable updated = objDut.GetTable("select * from " + table + " where ID=" + ID);
            InsertLogInDB("Update", table, ID);//Insert Executed Log Detail
            ExportLogInText(old, updated, table, ID); //Exported Log Data to .dat file see function for more detail
            return true;
        }
        else
        {
            return false;
        }
        //end
    }

    public bool UpdateDataByQuery(string[,] column_values, string ID, string table)
    {
        DataTable old = objDut.GetTable("select * from " + table + " where ID=" + ID);
        bool res = objDut.update(column_values, "ID=" + ID, table);
        if (res)
        {
            DataTable updated = objDut.GetTable("select * from " + table + " where ID=" + ID);
            InsertLogInDB("Update", table, ID);//Insert Executed Log Detail
            ExportLogInText(old, updated, table, ID); //Exported Log Data to .dat file see function for more detail
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool UpdateDataByQuery(string FullQuery, string ID, string table)
    {
        DataTable old = objDut.GetTable("select * from " + table + " where ID=" + ID);
        int res = objDut.ExecuteSql(FullQuery);
        if (res > 0)
        {
            DataTable updated = objDut.GetTable("select * from " + table + " where ID=" + ID);
            InsertLogInDB("Update", table, ID);//Insert Executed Log Detail
            ExportLogInText(old, updated, table, ID); //Exported Log Data to .dat file see function for more detail
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool UpdateDataByQuery(string FullQuery)
    {
        int res = objDut.ExecuteSql(FullQuery);
        if (res > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Delete(string ID, string table)
    {
        string[,] column_values = new string[,] { { "Status", "false" } };
        bool res = objDut.update(column_values, "ID=" + ID, table);
        if (res)
        {
            InsertLogInDB("Delete", table, ID);//Insert Executed Log Detail
            return true;
        }
        else
        {
            return false;
        }
    }

    public int ExecuteSQL(string query)
    {
        return objDut.ExecuteSql(query);
    }

    public bool ExecuteSQL(string query, params SqlParameter[] par)
    {
        int i = objDut.RunCommand(query, par);
        return i > 0 ? true : false;
    }

    /// <summary>
    /// This Function is Creating SqlParameter Object
    /// </summary>
    /// <param name="ParameterName"> ParameterName Name</param>
    /// <param name="Value"> ParameterName Value</param>
    /// <returns> SqlParameter</returns>
    public SqlParameter par(string ParameterName, string Value)
    {
        SqlParameter par = new SqlParameter();
        par.ParameterName = ParameterName;
        par.Value = Value;
        return par;
    }

    /// <summary>
    /// This Function is Creating SqlParameter Object
    /// </summary>
    /// <param name="ParameterName"> ParameterName Name</param>
    /// <param name="Value"> ParameterName Value</param>
    /// <returns> SqlParameter</returns>
    public SqlParameter par(string ParameterName, object Value)
    {
        SqlParameter par = new SqlParameter();
        par.ParameterName = ParameterName;
        par.Value = Value;
        return par;
    }

    /// <summary>
    /// This Function is Creating SqlParameter Object
    /// </summary>
    /// <param name="ParameterName"> ParameterName Name</param>
    /// <param name="Value"> ParameterName Value</param>
    /// <param name="SQLDBType"> SQLDBType</param>
    /// <returns> SqlParameter</returns>
    public SqlParameter par(string ParameterName, object Value, SqlDbType SQLDBType)
    {
        SqlParameter par = new SqlParameter();
        par.ParameterName = ParameterName;
        par.Value = Value;
        par.SqlDbType = SQLDBType;
        return par;
    }

    /// <summary>
    /// This Function is Creating Output SqlParameter Object
    /// </summary>
    /// <param name="ParameterName"> ParameterName Name</param>
    /// <param name="SQLDBType"> SQLDBType</param>
    /// <returns> SqlParameter</returns>
    public SqlParameter par_out(string ParameterName, SqlDbType SQLDBType)
    {
        SqlParameter par = new SqlParameter();
        par.ParameterName = ParameterName;
        par.Direction = ParameterDirection.Output;
        par.SqlDbType = SQLDBType;
        par.Size = 100;
        return par;
    }

    /// <summary>
    /// This Function is for Geting Next InsertedID for Given Table
    /// </summary>
    /// <param name="tableName"> SQL Table Name</param>
    /// <returns> string</returns>
    public string getNextID(string tableName)
    {
        string id = objDut.GetString("select ISNULL(MAX(ID),0)+1 from " + tableName);

        return id;
    }

    /// <summary>
    /// This Function is for Geting Maximun or last ID for Given Table
    /// </summary>
    /// <param name="tableName"> SQL Table Name</param>
    /// <returns> string</returns>
    public string getMaxID(string tableName)
    {
        string id = objDut.GetString("select max(ID) from " + tableName);
        if (string.IsNullOrEmpty(id))
        {
            return "0";
        }
        else
        {
            return id;
        }
    }

    /// <summary>
    /// This Function is for Geting Maximun or last ID for Given Table
    /// </summary>
    /// <param name="tableName"> SQL Table Name</param>
    /// <param name="Filter"> Where Condition '' Note:-(Don't Use Where Keyword)</param>
    /// <returns> string</returns>
    public string getMaxID(string tableName, string Filter)
    {
        string id = objDut.GetString("select max(ID) from " + tableName + " Where " + Filter);
        if (string.IsNullOrEmpty(id))
        {
            return "0";
        }
        else
        {
            return id;
        }
    }


    public bool IsExists(string table, string column, string value)
    {
        string query = "select top 1 * from " + table + " Where " + column + "='" + value + "' and status=1";
        DataTable dt = objDut.GetTable(query);
        if (dt.Rows.Count > 0)
            return true;
        else
            return false;
    }

    public bool IsExists(string table, string[,] Column_Values)
    {
        string query = "select top 1 * from " + table + " Where ";
        for (int i = 0; i < Column_Values.Length / 2; i++)
        {
            query += "[" + Column_Values[i, 0] + "]" + "='" + Column_Values[i, 1] + "' and ";
        }
        query = query.Remove(query.Length - 4);
        DataTable dt = objDut.GetTable(query);
        if (dt.Rows.Count > 0)
            return true;
        else
            return false;
    }

    /// <summary>
    /// This Function is for Getting image in byte[] from given filepath
    /// </summary>
    /// <param name="FullPath"> Full filepath of the image to be converted into byte[]</param>
    /// <returns>byte[]</returns>
    public byte[] ReadImage(string FullPath)
    {
        byte[] data = null;
        FileInfo fInfo = new FileInfo(FullPath);
        long numBytes = fInfo.Length;
        FileStream fStream = new FileStream(FullPath, FileMode.Open, FileAccess.Read);
        BinaryReader br = new BinaryReader(fStream);
        data = br.ReadBytes((int)numBytes);
        return data;
    }

    /// <summary>
    /// This Function is for Getting all Columns and rows of a SQL table given in Parameter
    /// </summary>
    /// <param name="table"> SQl Table Name in string</param>
    /// <returns>DataTable</returns>
    public DataTable GetAllData(string table)
    {
        return objDut.GetTable("select * from " + table + " where status=1");
    }

    /// <summary>
    /// This Function is for Getting all Columns and rows of a SQL table given in Parameter
    /// </summary>
    /// <param name="table"> SQl Table Name in string</param>
    /// <param name="CheckStatus"> true if you want to check status=1; false if don't</param>
    /// <returns>DataTable</returns>
    public DataTable GetAllData(string table, bool CheckStatus)
    {
        if (CheckStatus)
        {
            return objDut.GetTable("select * from " + table + " where status=1");
        }
        else
        {
            return objDut.GetTable("select * from " + table);
        }
    }

    /// <summary>
    /// This Function is for Getting a DataRow of paricular rows (ID) of a SQL table given in Parameter
    /// </summary>
    /// <param name="table"> SQl Table Name in string</param>
    /// <param name="ID"> ID of Row in the Table</param>
    /// <returns>DataRow</returns>
    public DataRow GetAllData(string table, string ID)
    {
        DataTable dt = objDut.GetTable("select * from " + table + " where status=1 and ID=" + ID);
        DataRow dr = dt.Rows[0];
        return dr;
    }

    /// <summary>
    /// This Function is for Getting a DataRow of paricular rows (ID) of a SQL table from a QUERY given in Parameter
    /// </summary>
    /// <param name="query"> SQl Query in string</param>
    /// <param name="ID"> ID of Row in the Table</param>
    /// <returns>DataRow</returns>
    public DataRow GetData(string query, string ID)
    {
        DataTable dt = objDut.GetTable(query + " where status=1 and ID=" + ID);
        DataRow dr = dt.Rows[0];
        return dr;
    }

    /// <summary>
    /// This Function is for Getting a DataTable from a QUERY given in Parameter
    /// </summary>
    /// <param name="table"> SQl query in string</param>
    /// <returns>DataTable</returns>
    public DataTable GetData(string query)
    {
        DataTable dt = objDut.GetTable(query);
        return dt;
    }

    /// <summary>
    /// This Function is for Getting a DataTable from a QUERY with Using Parameter
    /// </summary>
    /// <param name="query">T-Sql Query COntaining Parameter '@par'</param>
    /// <param name="par">SQl Paramers.</param>
    /// <returns></returns>
    public DataTable GetData(string query, params SqlParameter[] par)
    {
        DataTable dt = objDut.GetTable(query, par);
        return dt;
    }

    public string numbersOnly(string text)
    {
        if (Regex.IsMatch(text, "[^0-9]"))
        {
            text = text.Remove(text.Length - 1);
            return text;
        }
        else return text;
    }

    public bool IsNumber(string text)
    {
        bool res = !Regex.IsMatch(text, "[^0-9]");
        return res;
    }


    public bool IsInteger(string text)
    {
        int i;
        return int.TryParse(text, out i);
    }

    /************Regex Hint*************
    "[^0-9A-Za-z ]"
    The brackets specify a set of characters
    ^  : Just like !(Not)
    0-9: All digits
    A-Z: All capital letters
    a-z: All lowercase letters
    ' ': Spaces
    **********************************/

    public bool IsFloat(string text_value)
    {
        // Do not allow spaces.
        if (text_value.Contains(' ')) return false;
        // Allow a blank value.
        if (text_value.Length == 0) return true;
        // If the text doesn't end in a digit, add a digit
        // to see if it is a valid prefix of a float.
        if (!char.IsDigit(text_value, text_value.Length - 1))
            text_value += "0";
        // See if the text parses.
        float test_value;
        return float.TryParse(text_value, out test_value);
    }

    public bool IsFloat(string text_value, out float value)
    {
        // Do not allow spaces.
        if (text_value.Contains(' '))
        {
            value = 0.0f;
            return false;
        }
        // Allow a blank value.
        if (text_value.Length == 0)
        {
            value = 0.0f;
            return true;
        }
        // If the text doesn't end in a digit, add a digit
        // to see if it is a valid prefix of a float.
        if (!char.IsDigit(text_value, text_value.Length - 1))
            text_value += "0";
        // See if the text parses.
        return float.TryParse(text_value, out value);
    }

    public bool IsEmail(string text_value)
    {
        Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        Match match = regex.Match(text_value);
        if (match.Success)
            return true;
        else
            return false;
    }

    public bool isContainsSpecialChars(string value)
    {
        var list = new[] { "~", "`", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "+", "=", "\"" };
        return list.Any(value.Contains);
    }

    public bool dataCheck(DataTable dt, string column, string value)
    {
        //True if data is available
        foreach (DataRow dr in dt.Rows)
        {
            if (dr[column].ToString() == value)
            {
                return true;
            }
        }
        return false;
    }

    public bool dataCheck(DataTable dt, string column, string value, out DataRow dataRow)
    {
        //True if data is available
        foreach (DataRow dr in dt.Rows)
        {
            if (dr[column].ToString() == value)
            {
                dataRow = dr;
                return true;
            }
        }
        dataRow = null;
        return false;
    }

    public bool dataCheck(String TableName, string column, string value)
    {
        //True if data is available
        DataTable dt = GetData("select Top 1 " + column + " from " + TableName + " where " + column + "='" + value + "' and status=1");
        if (dt.Rows.Count > 0)
            return true;
        return false;
    }

    public bool dataCheck(String TableName, string column, string value, out DataRow dr)
    {
        //True if data is available
        DataTable dt = GetData("select * from " + TableName + " where " + column + "='" + value + "' and status=1");
        if (dt.Rows.Count > 0)
        {
            dr = dt.Rows[0];
            return true;
        }
        else
        {
            dr = null;
            return false;
        }
    }

    /// <summary>
    /// Get a First Row From DataTable Where [column]=[value]
    /// </summary>
    /// <param name="dt">DataTable</param>
    /// <param name="column">Column Available in DataTable</param>
    /// <param name="value">Value To Check</param>
    /// <returns>DataRow</returns>
    public DataRow GetRow(DataTable dt, string column, string value)
    {
        if (dt.Columns.Contains(column))
        {
            string filter = "[" + column + "]='" + value + "'";
            DataRow[] drs = dt.Select(filter);
            if (drs != null && drs.Length > 0)
                return drs[0];
            else
                return null;
        }
        else return null;
    }

    /// <summary>
    /// Get a String Value from a [Column] of a DataTable Where PrimaryKey=[PrimarKey]
    /// </summary>
    /// <param name="dt">Source DataTable</param>
    /// <param name="column">[Column] Whose Value is Needed</param>
    /// <param name="PrimarKey"> Unique PrimaryKey Value (Note:- First You Have to Set PrimaryKey Column of Databale Before Using This Function</param>
    /// <returns>string</returns>
    public string GetString(DataTable dt, string column, object PrimarKey)
    {
        if (dt.Rows.Count > 0)
        {
            DataRow dr = dt.Rows.Find(PrimarKey);
            if (dr != null && dt.Columns.Contains(column))
            {
                return dr[column].ToString();
            }
            else
                return null;
        }
        else
            return null;
    }

    public string GetString(string Query)
    {
        return objDut.GetString(Query);
    }

    public string AmountInRupee(int number)
    {
        return "Rs. " + AmountInWords(number) + " Only";
    }

    public string AmountInWords(int number)
    {
        return ConvertInWords(number);
    }

    private string ConvertInWords(int number)
    {
        if (number == 0) return "Zero";
        if (number < 0) return "minus " + AmountInWords(Math.Abs(number));
        string words = "";
        if ((number / 100000000) > 0)
        {
            words += AmountInWords(number / 10000000) + " Crore ";
            number %= 10000000;
        }
        if ((number / 1000000) > 0)
        {
            words += AmountInWords(number / 100000) + " Lakh ";
            number %= 100000;
        }
        if ((number / 1000) > 0)
        {
            words += AmountInWords(number / 1000) + " Thousand ";
            number %= 1000;
        }
        if ((number / 100) > 0)
        {
            words += AmountInWords(number / 100) + " Hundred ";
            number %= 100;
        }
        if (number > 0)
        {
            if (words != "") words += "and ";
            var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
            if (number < 20) words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0) words += " " + unitsMap[number % 10];
            }
        }
        return words;

    }

    public DataTable AddSNoColumn(DataTable dt)
    {
        DataColumn dc = new DataColumn();
        dc.ColumnName = "SNo";
        dc.DataType = typeof(int);
        if (!dt.Columns.Contains("SNo"))
        {
            dt.Columns.Add(dc);
            dc.SetOrdinal(0);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i][0] = i + 1;
            }
        }
        return dt;
    }

    public DataTable AddSNoColumn(DataTable dt, string col_Name)
    {
        DataColumn dc = new DataColumn();
        dc.ColumnName = col_Name;
        if (!dt.Columns.Contains(col_Name))
        {
            dt.Columns.Add(dc);
            dc.SetOrdinal(0);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i][0] = (i + 1).ToString("00");
            }
        }
        return dt;
    }

    public void SaveTextToFile(string Text, string fullFileName)
    {
        FileInfo file = new FileInfo(fullFileName);
        if (!file.Directory.Exists)
        {
            file.Directory.Create();
        }
        using (StreamWriter sw = new StreamWriter(fullFileName))
        {
            sw.Write(Text);
        }
    }

    public DataTable GetDataTable(DataRow[] DataRow)
    {
        DataTable dt = new DataTable();
        foreach (DataRow dr in DataRow)
        {
            dt.ImportRow(dr);
        }
        return dt;
    }

    public DataTable GetDataTable(DataTable dt, params string[] column)
    {
        DataView dv = new DataView(dt);
        DataTable res = dv.ToTable(false, column);
        return res;
    }

    /// <summary>
    /// Get a Smaller DataTable from given Table with RowFiltered and Selectd Column
    /// </summary>
    /// <param name="filter">DataView RowFilter</param>
    /// <param name="dt">Source DataTable</param>
    /// <param name="columns">Columns Name Seperated By Commma</param>
    /// <returns></returns>
    public DataTable GetDataTable(string filter, DataTable dt, string columns)
    {
        DataView dv = new DataView(dt);
        dv.RowFilter = filter;
        List<string> ColumnList = new List<string>();
        if (columns.Contains(','))
        {
            ColumnList.AddRange(columns.Split(',').ToList().Select(x => x.Trim()));
        }
        else
        {
            ColumnList.Add(columns);
        }
        DataTable res = dv.ToTable(false, ColumnList.ToArray());
        return res;
    }

    /// <summary>
    /// Get a Smaller DataTable from given Table with RowFiltered and All Columns2017
    /// </summary>
    /// <param name="filter">DataView RowFilter</param>
    /// <param name="dt">Source DataTable</param>
    /// <returns></returns>
    public DataTable GetDataTable(string filter, DataTable dt)
    {
        DataView dv = new DataView(dt);
        dv.RowFilter = filter;
        DataTable res = dv.ToTable();
        return res;
    }

    /// <summary>
    /// Get a Smaller DataTable from given Table with RowFiltered
    /// </summary>
    /// <param name="filter">DataView RowFilter</param>
    /// <param name="dt">Source DataTable</param>
    /// <param name="isDistinct">Provide Only Distinct Value if True</param>
    /// <param name="columns">List of Columns Seoerated by (,) or Provide (*) for All Columns</param>
    /// <returns></returns>
    public DataTable GetDataTable(string filter, DataTable dt, bool isDistinct, string columns)
    {
        DataView dv = new DataView(dt);
        dv.RowFilter = filter;

        List<string> ColumnList = new List<string>();
        if (columns.Contains('*'))
        {
            ColumnList.AddRange(dt.Columns.OfType<DataColumn>().Select(x => x.ColumnName).ToArray());
        }
        else//If * in not Include in It
        {
            if (columns.Contains(',')) ColumnList.AddRange(columns.Split(',').ToList().Select(x => x.Trim()));
            else ColumnList.Add(columns);
        }
        DataTable res = dv.ToTable(isDistinct, ColumnList.ToArray());
        return res;
    }

    public string Join(DataTable dt, string column, string separator)
    {
        string joined = "";
        if (dt.Rows.Count > 0)
        {
            var list = dt.Rows.OfType<DataRow>().Select(x => x[column].ToString());
            HashSet<string> set = new HashSet<string>(list);//Removing Duplicate using set
            joined = String.Join(separator, set);
        }
        return joined;
    }

    public bool SaveFile(byte[] file, string FullPath)
    {
        bool success = false;
        string path = FullPath.Remove(FullPath.LastIndexOf('.'));
        string ext = FullPath.Substring(FullPath.LastIndexOf('.'));

        objIO.saveFile(path, ext.Replace(".", ""), file);
        return success;
    }

    public DataTable RemoveColumns(DataTable dt, params string[] columns)
    {
        foreach (var column in columns)
        {
            dt.Columns.Remove(column);
        }

        return dt;
    }

    public List<int[]> NavigationRange(int TotalRecord, int NoOfEachGroup)
    {
        List<int[]> navRange = new List<int[]>();
        int cnt = 1;
        for (int i = 0; i < TotalRecord / NoOfEachGroup; i++)
        {
            int[] range = new int[NoOfEachGroup];
            for (int j = 0; j < NoOfEachGroup; j++)
            {
                range[j] = cnt;
                cnt++;
            }
            navRange.Add(range);
        }
        if (TotalRecord % NoOfEachGroup > 0)
        {
            int[] range = new int[TotalRecord % NoOfEachGroup];
            for (int i = 0; i < TotalRecord % NoOfEachGroup; i++)
            {
                range[i] = cnt;
                cnt++;
            }
            navRange.Add(range);
        }
        return navRange;
    }

    public string Encrypt(string Text)
    {
        string result = "";
        result = Encrypt(Text, EncryptionKey);
        return result;
    }
    //private static string SpecialChar = " ~!@#$%^&*()_+{}|:<>?-=[],.";
    //private static string NormalChar = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public string Decrypt(string Text)
    {
        string result = "";
        result = Decrypt(Text, EncryptionKey);
        return result;
    }

    // This constant is used to determine the keysize of the encryption algorithm in bits.
    // We divide this by 8 within the code below to get the equivalent number of bytes.
    private const int Keysize = 256;

    // This constant determines the number of iterations for the password bytes generation function.
    private const int DerivationIterations = 1000;

    public static string Encrypt(string plainText, string passPhrase)
    {
        // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
        // so that the same Salt and IV values can be used when decrypting.  
        var saltStringBytes = Generate256BitsOfRandomEntropy();
        var ivStringBytes = Generate256BitsOfRandomEntropy();
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                            var cipherTextBytes = saltStringBytes;
                            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                            memoryStream.Close();
                            cryptoStream.Close();
                            return Convert.ToBase64String(cipherTextBytes);
                        }
                    }
                }
            }
        }
    }

    public static string Decrypt(string cipherText, string passPhrase)
    {
        // Get the complete stream of bytes that represent:
        // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
        var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
        // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
        var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
        // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
        var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
        // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
        var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream(cipherTextBytes))
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var plainTextBytes = new byte[cipherTextBytes.Length];
                            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                            memoryStream.Close();
                            cryptoStream.Close();
                            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                        }
                    }
                }
            }
        }
    }

    private static byte[] Generate256BitsOfRandomEntropy()
    {
        var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
        using (var rngCsp = new RNGCryptoServiceProvider())
        {
            // Fill the array with cryptographically secure random bytes.
            rngCsp.GetBytes(randomBytes);
        }
        return randomBytes;
    }

    public bool EncryptFile(string inputFilePath, string outputfilePath)
    {
        try
        {
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (FileStream fsOutput = new FileStream(outputfilePath, FileMode.Create))
                {
                    using (CryptoStream cs = new CryptoStream(fsOutput, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (FileStream fsInput = new FileStream(inputFilePath, FileMode.Open))
                        {
                            int data;
                            while ((data = fsInput.ReadByte()) != -1)
                            {
                                cs.WriteByte((byte)data);
                            }
                        }
                    }
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
    string EncryptionKey = "fillip_infoak";
    public bool DecryptFile(string inputFilePath, string outputfilePath)
    {
        try
        {
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (FileStream fsInput = new FileStream(inputFilePath, FileMode.Open))
                {
                    using (CryptoStream cs = new CryptoStream(fsInput, encryptor.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (FileStream fsOutput = new FileStream(outputfilePath, FileMode.Create))
                        {
                            int data;
                            while ((data = cs.ReadByte()) != -1)
                            {
                                fsOutput.WriteByte((byte)data);
                            }
                        }
                    }
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public DateTime GetOnlineDate()
    {
        DateTime OnlineDate;
        var client = new System.Net.Sockets.TcpClient("time.nist.gov", 13);
        if (CheckForInternetConnection())
        {
            using (var streamReader = new System.IO.StreamReader(client.GetStream()))
            {
                var response = streamReader.ReadToEnd();
                var utcDateTimeString = response.Substring(7, 17);
                OnlineDate = DateTime.ParseExact(utcDateTimeString, "yy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal);
            }
            return OnlineDate;
        }
        else
            return DateTime.Now;
    }

    public bool CheckForInternetConnection()
    {
        try
        {
            using (var client = new WebClient())
            {
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }
    }

    public string GetTextFromURL(string URL)
    {
        string ReturnText = "";
        using (WebClient wc = new WebClient())
        {
            ReturnText = wc.DownloadString(URL);
        }
        return ReturnText;
    }

    public bool GetTextFromURL(string URL, out string ReturnText)
    {
        if (CheckForInternetConnection())
        {
            ReturnText = "";
            using (WebClient wc = new WebClient())
            {
                ReturnText = wc.DownloadString(URL);
            }
            return true;
        }
        else
        {
            ReturnText = null;
            return false;
        }
    }

    public DataView FilterDate(DataTable dt, DateTime from, DateTime to, string DateColumn)
    {
        DataView dv = dt.DefaultView;
        dv.RowFilter = String.Format("[{0}] >= #{1}# and {0} <= #{2}#", DateColumn, from.ToString(), to.ToString());
        return dv;
    }

    public DataView FilterDate(DataTable dt, DateTime dateTime, string DateColumn)
    {
        DataView dv = dt.DefaultView;
        dv.RowFilter = String.Format("[{0}] = #{1}#", DateColumn, dateTime.ToString());
        return dv;
    }

    public DataView FilterText(DataTable dt, string column, string value)
    {
        DataView dv = dt.DefaultView;
        //concat if multiple columns are available
        if (column.Contains('+'))
        {
            string[] columnList = column.Split('+');
            column = "[";
            foreach (string col in columnList)
            {
                column += col.Trim() + "] + [";
            }
            column = column.Remove(column.Length - 4);
        }
        else
        {
            column = "[" + column + "]";
        }
        //
        dv.RowFilter = String.Format("{0} Like '%{1}%'", column, EscapeLike(value.Trim()));
        return dv;
    }

    public DataView FilterText(DataTable dt, string column, string value, DateTime from, DateTime to, string dateColumn)
    {
        DataView dv = dt.DefaultView;
        //concat if multiple columns are available
        if (column.Contains('+'))
        {
            string[] columnList = column.Split('+');
            column = "[";
            foreach (string col in columnList)
            {
                column += col.Trim() + "] + [";
            }
            column = column.Remove(column.Length - 4);
        }
        else
        {
            column = "[" + column + "]";
        }
        //
        dv.RowFilter = String.Format("{0} Like '%{1}%' AND [{2}] >= #{3}# AND [{2}] <= #{4}#", column, EscapeLike(value.Trim()), dateColumn, from.ToString(), to.ToString());
        return dv;
    }

    //Escape the single quote ' by doubling it to ''. Escape * % [ ] characters by wrapping in []. e.g.
    public string EscapeLike(string value)
    {
        StringBuilder sb = new StringBuilder(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            switch (c)
            {
                case ']':
                case '[':
                case '%':
                case '*':
                    sb.Append("[").Append(c).Append("]");
                    break;
                case '\'':
                    sb.Append("''");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Get full temproryFile path with Extension given in Parameter for temporary Storage
    /// </summary>
    /// <param name="extension">Extension of the File to Save(Without .)</param>
    /// /// <returns>Full Temprory Path</returns>
    public string GetTempFilePath(string extension)
    {
        //get temprory path(%temp%)
        string fullpath = Path.GetTempPath();
        //adding unique identifier name
        fullpath += Guid.NewGuid();
        //adding extension
        fullpath += "." + extension;
        return fullpath;
    }

    /// <summary>
    /// Get full temproryFile path without Extension given in Parameter for temporary Storage
    /// </summary>
    /// /// <returns>Full Temprory Path</returns>
    public string GetTempFilePath()
    {
        //get temprory path(%temp%)
        string fullpath = Path.GetTempPath();
        //adding unique identifier name
        fullpath += Guid.NewGuid();
        return fullpath;
    }

    public bool checkTime(string dateString)
    {
        string[] timeFormats = new string[]
            {
                    "hh:mm tt", "hh:mm:ss tt",
                    "h:mm tt", "h:mm:ss tt",
                    "HH:mm:ss", "HH:mm", "H:mm"
            };
        // check to see if the date string has a time only
        DateTime dateTimeTemp;
        return DateTime.TryParseExact(dateString, timeFormats, CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out dateTimeTemp);
    }

    public void ExcuteTransactionalQuery(string qry)
    {
        //Excute Query
        if (qry != "")
        {
            string transactionalQuery = String.Format(@"BEGIN TRANSACTION;
	                                            BEGIN TRY
		                                            --Some code------
		                                            {0}
		                                            -----------------
		                                            COMMIT TRANSACTION;
	                                            END TRY
	                                            BEGIN CATCH

		                                            ROLLBACK TRANSACTION;
	                                            END CATCH;", qry);
            objDut.ExecuteSql(transactionalQuery);
        }
    }

    public bool DuplicateCheck(DataTable dt, string column)
    {
        bool hasDuplicate = false;
        HashSet<object> hashSet = new HashSet<object>();
        foreach (DataRow dr in dt.Rows)
        {
            if (hashSet.Contains(dr[column]))
            {
                hasDuplicate = true;
                break;
            }
            else
                hashSet.Add(dr[column]);
        }
        return hasDuplicate;
    }




}