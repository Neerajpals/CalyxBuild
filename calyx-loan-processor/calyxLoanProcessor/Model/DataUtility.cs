
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Diagnostics;

class DataUtility
{
    #region AllProperties

    private SqlConnection _con;

    public SqlConnection con
    {
        get { return _con; }
        set { _con = value; }
    }
    private SqlCommand _cmd;

    public SqlCommand cmd
    {
        get { return _cmd; }
        set { _cmd = value; }
    }
    private SqlDataReader _dr;

    public SqlDataReader dr
    {
        get { return _dr; }
        set { _dr = value; }
    }
    private SqlDataAdapter _sda;

    public SqlDataAdapter sda
    {
        get { return _sda; }
        set { _sda = value; }
    }
    private DataTable _dt;

    public DataTable dt
    {
        get { return _dt; }
        set { _dt = value; }
    }
    private DataSet _ds;

    public DataSet ds
    {
        get { return _ds; }
        set { _ds = value; }
    }


    #endregion

    #region All PrivateMethod(s)
    /// <summary>
    /// This is a Function to Open Connection
    /// </summary>
    private void OpenConnection()
    {
        if (_con == null)
        {
            _con = new SqlConnection(ConfigurationManager.AppSettings["ConnectionString"]);
        }
        if (_con.State == ConnectionState.Closed)
        {
            _con.Open();
        }
        _cmd = new SqlCommand();
        _cmd.Connection = _con;
    }
    /// <summary>
    /// This is a Function to Close the Connection
    /// </summary>
    private void CloseConnection()
    {
        if (_con.State == ConnectionState.Open)
        {
            _con.Close();
        }
    }
    /// <summary>
    /// This is a Function to Dispose the Connection.
    /// </summary>
    private void DisposeConnection()
    {
        if (_con != null)
        {
            _con.Dispose();
            _con = null;
        }

    }
    #endregion

    #region All PublicMethod(s)
    /// <summary>
    /// This is a Function to Execute DML
    /// </summary>
    /// <param name="strsql"></param>
    /// <returns></returns>
    public int ExecuteSql(string strsql)
    {
        OpenConnection();
        _cmd.CommandType = CommandType.Text;
        _cmd.CommandText = strsql;
        //_cmd.CommandTimeout = 30;
        int result = _cmd.ExecuteNonQuery();
        CloseConnection();
        return result;
    }

    /// <summary>
    /// This is a Function to Execute DML using StoredProcedure.
    /// </summary>
    /// <param name="SPName"></param>
    /// <param name="arrparam"></param>
    /// <returns></returns>
    public int ExecuteSql(String SPName, SqlParameter[] arrparam)
    {
        OpenConnection();
        _cmd.CommandType = CommandType.StoredProcedure;
        _cmd.CommandText = SPName;
        if (_cmd.Parameters.Count > 0)
        {
            _cmd.Parameters.Clear();
        }
        if (arrparam != null)
        {
            foreach (SqlParameter param in arrparam)
            {
                if (arrparam != null)
                    _cmd.Parameters.Add(param);
            }
        }
        int result = _cmd.ExecuteNonQuery();
        CloseConnection();
        DisposeConnection();
        return result;
    }

    /// <summary>
    /// This is a Function to Execute DML using Parameter.
    /// </summary>
    /// <param name="Command"> Sql Command Text</param>
    /// <param name="arrparam"> Parameter Collection</param>
    /// <returns></returns>
    public int RunCommand(String Command, SqlParameter[] arrparam)
    {
        OpenConnection();
        _cmd.CommandType = CommandType.Text;
        _cmd.CommandText = Command;
        if (_cmd.Parameters.Count > 0)
        {
            _cmd.Parameters.Clear();
        }
        if (arrparam != null)
        {
            foreach (SqlParameter param in arrparam)
            {
                if (arrparam != null)
                    _cmd.Parameters.Add(param);
            }
        }
        int result = _cmd.ExecuteNonQuery();
        CloseConnection();
        DisposeConnection();
        return result;
    }

    public int ExecSql(String SPName, SqlParameter[] arrparam)
    {
        OpenConnection();
        _cmd.CommandType = CommandType.StoredProcedure;
        _cmd.CommandText = SPName;
        if (_cmd.Parameters.Count > 0)
        {
            _cmd.Parameters.Clear();
        }
        if (arrparam != null)
        {
            for (int i = 0; i < arrparam.Length; i++)
            {
                if (arrparam[i] != null)
                {
                    _cmd.Parameters.Add(arrparam[i]);
                    //if (arrparam[i].Value == DBNull.Value)
                    //{
                    //    _cmd.Parameters.Add(arrparam[i].ParameterName,arrparam[i].SqlDbType);
                    //}
                    //else
                    //{
                    //    _cmd.Parameters.AddWithValue(arrparam[i].ParameterName, arrparam[i].Value);
                    //}
                }
            }
        }
        int result = _cmd.ExecuteNonQuery();
        CloseConnection();
        DisposeConnection();
        return result;
    }

    public int ExecuteSql1(String ProcName, SqlParameter[] arrparam)
    {
        OpenConnection();
        _cmd.CommandType = CommandType.StoredProcedure;
        _cmd.CommandText = ProcName;
        if (_cmd.Parameters.Count > 0)
        {
            _cmd.Parameters.Clear();
        }
        if (arrparam != null)
        {
            foreach (SqlParameter param in arrparam)
            {
                if (param.Direction == ParameterDirection.Output)
                {
                    _cmd.Parameters.Add(param);
                }
                else
                {
                    _cmd.Parameters.AddWithValue(param.ParameterName, param.Value);
                }
            }
        }
        int result = _cmd.ExecuteNonQuery();
        CloseConnection();
        DisposeConnection();
        return result;
    }

    /// <summary>
    /// This is a Function to GetData Using SqlDatareder
    /// </summary>
    /// <param name="strSql"></param>
    /// <returns></returns>
    public SqlDataReader GetData(string strSql)
    {
        OpenConnection();
        _cmd.CommandType = CommandType.Text;
        //_cmd.CommandTimeout = 30;
        _cmd.CommandText = strSql;
        dr = _cmd.ExecuteReader();
        return dr;
    }
    /// <summary>
    /// This is a function to GetData Using Stored Procedure
    /// </summary>
    /// <param name="strSql"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public SqlDataReader GetData(string strSql, SqlParameter param)
    {
        OpenConnection();
        _cmd.CommandType = CommandType.StoredProcedure;
        _cmd.Parameters.Add(param);
        //_cmd.CommandTimeout = 30;
        _cmd.CommandText = strSql;
        dr = _cmd.ExecuteReader();
        return dr;
    }
    /// <summary>
    /// This is a parameterized function to access the data to the datareader
    /// </summary>
    /// <param name="strSPname"></param>
    /// <param name="arrparams"></param>
    /// <returns></returns>
    public SqlDataReader Getdatareader(string strsql, SqlParameter[] arrparams)
    {
        OpenConnection();
        //dReader=new SqlDataReader();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = strsql;
        //cmd.CommandTimeout = 30;
        //This is to clean the existing values lies on parameters property
        if (cmd.Parameters.Count > 0)
        {
            cmd.Parameters.Clear();
        }
        if (arrparams != null)
        {
            foreach (SqlParameter param in arrparams)
            {
                cmd.Parameters.Add(param);
            }
        }
        dr = cmd.ExecuteReader();
        return dr;
    }

    /// <summary>
    /// This is a Function to GetDataTable using DataTable with SqlDataAdapter
    /// </summary>
    /// <param name="strSql"></param>
    /// <returns></returns>
    public DataTable GetTable(string strSql)
    {
        OpenConnection();
        SqlDataAdapter da = new SqlDataAdapter();
        DataTable dt = new DataTable();
        _cmd.CommandType = CommandType.Text;
        _cmd.CommandText = strSql;
        da.SelectCommand = _cmd;
        da.Fill(dt);
        CloseConnection();
        DisposeConnection();
        return dt;
    }

    /// <summary>
    /// This is a Function to GetDataTable using Query Having Parameter
    /// </summary>
    /// <param name="strSql">T-Sql Query with Parameter '@'</param>
    /// /// <param name="parameters">List of Sql Parameters</param>
    /// <returns></returns>
    public DataTable GetTable(string strSql, params SqlParameter[] parameters)
    {
        OpenConnection();
        SqlDataAdapter da = new SqlDataAdapter();
        DataTable dt = new DataTable();
        _cmd.CommandType = CommandType.Text;
        _cmd.CommandText = strSql;

        if (_cmd.Parameters.Count > 0)
            _cmd.Parameters.Clear();
        foreach (SqlParameter par in parameters)
        {
            _cmd.Parameters.Add(par);
        }

        da.SelectCommand = _cmd;
        da.Fill(dt);
        CloseConnection();
        DisposeConnection();
        return dt;
    }

    public DataTable GetTableAtten(string strSql)
    {
        OpenConnection();
        SqlDataAdapter mda = new SqlDataAdapter();
        DataTable dt1 = new DataTable();
        //_cmd.CommandType = CommandType.Text;
        _cmd.CommandType = CommandType.StoredProcedure;
        _cmd.Parameters.AddWithValue("@date", DateTime.Now.Date);
        _cmd.CommandText = strSql;
        mda.SelectCommand = _cmd;
        mda.Fill(dt1);
        CloseConnection();
        DisposeConnection();
        return dt1;
    }
    public DataTable GetTable1(string strSql)
    {
        OpenConnection();
        SqlDataAdapter adp = new SqlDataAdapter();
        DataTable dt = new DataTable();
        _cmd.CommandType = CommandType.Text;
        _cmd.CommandText = strSql;
        adp.SelectCommand = _cmd;
        adp.Fill(dt);
        CloseConnection();
        DisposeConnection();
        return dt;
    }

    /// <summary>
    /// This is a parameterized function to access the data to the datatable from dataadopter.
    /// </summary>
    /// <param name="strSPname"></param>
    /// <param name="arrparams"></param>
    /// <returns></returns>
    public DataTable getdatatable(string strSPname, SqlParameter[] arrparams)
    {
        OpenConnection();
        //create datatable object
        DataTable dt = new DataTable();
        SqlCommand cmd = new SqlCommand();
        cmd.Connection = con;
        sda = new SqlDataAdapter();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = strSPname;
        if (cmd.Parameters.Count > 0)
        {
            cmd.Parameters.Clear();
        }
        if (arrparams != null)
        {
            foreach (SqlParameter param in arrparams)
            {
                cmd.Parameters.Add(param);
            }
        }
        sda.SelectCommand = cmd;
        sda.Fill(dt);
        CloseConnection();
        DisposeConnection();
        return dt;
    }

    public DataSet GetDataset(string strSql, string TableName, SqlParameter[] arrparams)
    {
        DataSet dss = new DataSet();
        SqlDataAdapter mda = new SqlDataAdapter();
        OpenConnection();
        _cmd.CommandType = CommandType.StoredProcedure;
        _cmd.CommandText = strSql;
        if (cmd.Parameters.Count > 0)
        {
            cmd.Parameters.Clear();
        }
        if (arrparams != null)
        {
            foreach (SqlParameter param in arrparams)
            {
                cmd.Parameters.Add(param);
            }
        }
        mda.SelectCommand = _cmd;
        mda.Fill(dss, TableName);
        CloseConnection();
        DisposeConnection();
        return dss;

    }
    public DataSet GetDataset(string strSql, string TableName)
    {
        DataSet dss = new DataSet();
        SqlDataAdapter mda = new SqlDataAdapter();
        OpenConnection();
        _cmd.CommandType = CommandType.Text;
        _cmd.CommandText = strSql;
        mda.SelectCommand = _cmd;
        mda.Fill(dss, TableName);
        CloseConnection();
        DisposeConnection();
        return dss;
    }

    public DataSet GetDataset(string strSql)
    {
        DataSet dss = new DataSet();
        SqlDataAdapter mda = new SqlDataAdapter();
        OpenConnection();
        _cmd.CommandType = CommandType.Text;
        _cmd.CommandText = strSql;
        mda.SelectCommand = _cmd;
        mda.Fill(dss);
        CloseConnection();
        DisposeConnection();
        return dss;
    }

    /*================================================Ge String====================================*/
    public String GetString(string strSql)
    {
        string get = "";
        SqlDataAdapter mda = new SqlDataAdapter();
        OpenConnection();
        _cmd.CommandType = CommandType.Text;
        _cmd.CommandText = strSql;
        if (_cmd.ExecuteScalar() != null && _cmd.ExecuteScalar().ToString() != "")
        {
            get = _cmd.ExecuteScalar().ToString();
        }
        CloseConnection();
        DisposeConnection();
        return get;
    }

    /// <summary>
    /// This is to Validate a record if it is already there in the table.
    /// </summary>
    public bool IsExists(string query)
    {
        DataTable chk = GetTable(query);
        if (chk.Rows.Count > 0)
            return true;
        else
            return false;
    }

    public bool isExist(string[,] values, string table, int j) // Double existence check Here mainly put j = 2 (if values is not present in table then return true otherwise false)
    {
        for (int i = 0; i < values.Length / 2; i = i + j)
        {
            string setv = "[" + values[i, 1] + "]" + "='" + values[i, 0] + "'" + " and " + "[" + values[i + 1, 1] + "]" + "='" + values[i + 1, 0] + "'";
            string query = "select * from " + table + " where " + setv;
            DataTable dt = GetTable(query);
            if (dt.Rows.Count > 0)
            {
                // MessageBox.Show(values[i, 1] + " & " + values[i+1, 1] + " Already Exist");
                return false;
            }
        }
        return true;
    }

    public bool has(string[,] values, string table) // Presense of value check (if values is present in table then return true otherwise false)
    {
        string setv = "";
        for (int i = 0; i < values.Length / 2; i = i + 1)
        {
            setv = setv + " [" + values[i, 1] + "]" + "='" + values[i, 0] + "'" + " ,";
        }
        setv = setv.Remove(setv.Length - 1);
        setv = setv.Replace(",", "and");
        string query = "select * from " + table + " where" + setv;
        DataTable dt = GetTable(query);
        if (dt.Rows.Count > 0)
        {
            return true;
        }
        return false;
    }

    public bool has(string[,] values, string table, out DataTable output) // Presense of value check (if values is present in table then return true otherwise false with output value)
    {
        string setv = "";
        for (int i = 0; i < values.Length / 2; i = i + 1)
        {
            setv = setv + " [" + values[i, 1] + "]" + "='" + values[i, 0] + "'" + " ,";
        }
        setv = setv.Remove(setv.Length - 1);
        setv = setv.Replace(",", "and");
        string query = "select * from " + table + " where" + setv;
        DataTable dt = GetTable(query);
        if (dt.Rows.Count > 0)
        {
            output = dt;
            return true;
        }
        else
        {
            output = dt;
            return false;
        }
    }

    public bool insert(string[] cols, string[] value, string table)
    {
        string val = "";
        string col = "";
        foreach (String v in value)
        {
            val += "'" + v + "',";
        }
        foreach (String v in cols)
        {
            col += v + ",";
        }
        val = val.Remove(val.Length - 1);
        col = col.Remove(col.Length - 1);
        string query = "insert into " + table + " (" + col + ") values (" + val + ")";
        int exec = ExecuteSql(query);
        if (exec > 0)
        {
            return true;
        }
        return false;
    }

    public bool insert(string[,] Column_Values, string table)
    {
        string val = "";
        string col = "";
        int i = 1;
        foreach (string s in Column_Values)
        {
            if (i % 2 == 0)
            {
                val += "'" + s + "',";
            }
            else
            {
                col += s + ",";
            }
            i++;
        }
        val = val.Remove(val.Length - 1);
        col = col.Remove(col.Length - 1);
        string query = "insert into " + table + " (" + col + ") values (" + val + ")";
        int exec = ExecuteSql(query);
        if (exec > 0)
            return true;
        return false;
    }

    public bool insert(string table, params SqlParameter[] par)
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
        int i = RunCommand(query, par);
        if (i > 0)
            return true;
        return false;
    }

    public bool Update(string table, string ID, params SqlParameter[] par)
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
        int i = RunCommand(query, par);
        if (i > 0)
            return true;
        return false;
    }

    public bool update(string[] cols, string[] value, string table, string condition)
    {
        string query = "Update " + table + "set ";
        if (cols.Length == value.Length)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                query += cols[i] + "='" + value[i] + "',";
            }
        }
        query = query.Remove(query.Length - 1);
        query += " where" + condition;
        int exec = ExecuteSql(query);
        if (exec > 0)
            return true;
        return false;
    }

    public bool update(string[,] Column_Values, string condition, string table)
    {
        int exec = 0;
        string setv = "";
        for (int i = 0; i < Column_Values.Length / 2; i++)
        {
            setv += "[" + Column_Values[i, 0] + "]" + "='" + Column_Values[i, 1] + "',";
        }
        setv = setv.Remove(setv.Length - 1);
        string query = "update " + table + " set " + setv + " where " + condition;
        exec += ExecuteSql(query);
        if (exec > 0)
            return true;
        return false;
    }

    #endregion
}

