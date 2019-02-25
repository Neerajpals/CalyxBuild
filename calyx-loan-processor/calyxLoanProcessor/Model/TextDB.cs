
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;

class TextDB
{

    #region All Public Method (For TSV Format)
    /// <summary>
    /// This is a Function to Get DataTable from a TSV(File)
    /// </summary>
    /// <param name="fileInfo"> FileInfo object of the File Specified</param>
    /// <returns></returns>
    public DataTable GetDataTable(FileInfo fileInfo)
    {
        //setting Default encoding if not given
        Encoding encoding = Encoding.UTF8;
        return GetDataTable(fileInfo, encoding);
    }

    /// <summary>
    /// This is a Function to Get DataTable from a TSV(File)
    /// </summary>
    /// <param name="fileInfo"> FileInfo object of the File Specified</param>
    /// <param name="encoding"> Encoding of Text in File</param>
    /// <returns></returns>
    public DataTable GetDataTable(FileInfo fileInfo, Encoding encoding)
    {
        byte[] stringBytes = null;
        //opening the file for reading
        using (BinaryReader br = new BinaryReader(File.Open(fileInfo.FullName, FileMode.Open)))
        {
            stringBytes = br.ReadBytes((int)fileInfo.Length);
        }
        return GetDataTable(stringBytes, encoding);
    }

    /// <summary>
    /// This is a Function to Get DataTable from a TSV(file) Converted into Bytes
    /// </summary>
    /// <param name="stringBytes"> Bytes of File Specified</param>
    /// <param name="encoding"> Encoding of Text in File</param>
    /// <returns></returns>
    public DataTable GetDataTable(byte[] stringBytes, Encoding encoding)
    {
        //Decoding Bytes into String via Passed Encoding codepage
        string tSVdata = encoding.GetString(stringBytes);
        return GetDataTable(tSVdata);
    }

    /// <summary>
    /// This is a Function to Get DataTable from a TSV String
    /// </summary>
    /// <param name="TSVdata"> TSV(Tab Separated Values) String</param>
    /// <returns></returns>
    public DataTable GetDataTable(string TSVdata)
    {
        DataTable dt = new DataTable();
        string[] rows = TSVdata.Split('\r');
        foreach (string row in rows)
        {
            if (row.ToCharArray()[0] != '\n')
            {
                string[] headers = row.Split('\t');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
            }
            else
            {
                string[] elements = row.Remove(0, 1).Split('\t');
                DataRow dr = dt.NewRow();
                int col = 0;
                foreach (string element in elements)
                {
                    dr[col] = element;
                    col++;
                }
                dt.Rows.Add(dr);
            }
        }
        return dt;
    }

    /// <summary>
    /// This is a Function to get TSV String from DataTable
    /// </summary>
    /// <param name="dt">DataTable</param>
    /// <returns></returns>
    public string ToTSV(DataTable dt)
    {
        string TsvString = "";
        foreach (DataColumn dc in dt.Columns)
        {
            TsvString += dc.ColumnName + "\t";
        }
        TsvString = TsvString.Remove(TsvString.Length - 1, 1);//Remove last \t
        TsvString += "\r\n";
        foreach (DataRow dr in dt.Rows)
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                TsvString += dr[i].ToString() + "\t";
            }
            TsvString = TsvString.Remove(TsvString.Length - 1, 1);//Remove last \t
            TsvString += "\r\n";
        }
        return TsvString;
    }

    /// <summary>
    /// This is a Function to Save file from DataTable
    /// </summary>
    /// <param name="dt">DataTable</param>
    /// <param name="fileName">Full file Path to Store Data</param>
    /// <returns></returns>
    public void SaveFile(DataTable dt, string fileName)
    {
        string TSVdata = ToTSV(dt);
        SaveFile(TSVdata, fileName);
    }

    /// <summary>
    /// This is a Function to Save file from a TSV String
    /// </summary>
    /// <param name="TSVdata"> TSV(Tab Separated Values) String</param>
    /// <param name="fileName">Full file Path to Store Data</param>
    /// <returns></returns>
    public void SaveFile(string TSVdata, string fileName)
    {
        //Encoding string data into Bytes to save File
        byte[] bytes = Encoding.UTF8.GetBytes(TSVdata);
        //creating file from Bytes
        using (BinaryWriter bw = new BinaryWriter(File.Create(fileName)))
        {
            bw.Write(bytes);
        }
    }

    /// <summary>
    /// This is a Function to Get String Between Two Unique String or CharCode
    /// </summary>
    /// <param name="From"> From String</param>
    /// <param name="To"> To String</param>
    /// <param name="Main"> Main String from which required string would be extracted</param>
    /// <returns></returns>
    public string GetMidString(string From, string To, string Main)
    {
        int iFrom = Main.IndexOf(From) + From.Length;
        int iTo = Main.LastIndexOf(To);

        string result = Main.Substring(iFrom, iTo - iFrom);
        return result;
    }
    #endregion

    #region All Public Method (For JSON Format)

    /// <summary>
    /// This is a Function to Get DataTable from a JSON String
    /// </summary>
    /// <param name="jsonString"> JSON String </param>
    /// <returns></returns>
    public DataTable JSONTable(string jsonString)
    {
        try
        {
            DataTable dt = new DataTable();
            //strip out bad characters
            string[] jsonParts = Regex.Split(jsonString.Replace("[", "").Replace("]", ""), "},{");

            //hold column names
            List<string> dtColumns = new List<string>();

            //get columns
            foreach (string jp in jsonParts)
            {
                //only loop thru once to get column names
                string[] propData = Regex.Split(jp.Replace("{", "").Replace("}", ""), ",");
                foreach (string rowData in propData)
                {
                    try
                    {
                        int idx = rowData.IndexOf(":");
                        string n = rowData.Substring(0, idx - 1);
                        string v = rowData.Substring(idx + 1);
                        if (!dtColumns.Contains(n))
                        {
                            dtColumns.Add(n.Replace("\"", ""));
                        }
                    }
                    catch
                    {
                        throw new Exception(string.Format("Error Parsing Column Name : {0}", rowData));
                    }

                }
                break; // TODO: might not be correct. Was : Exit For
            }
            //build dt
            foreach (string c in dtColumns)
            {
                dt.Columns.Add(c);
            }
            //get table data
            foreach (string jp in jsonParts)
            {
                string[] propData = Regex.Split(jp.Replace("{", "").Replace("}", ""), ",");
                DataRow nr = dt.NewRow();
                foreach (string rowData in propData)
                {
                    try
                    {
                        int idx = rowData.IndexOf(":");
                        string n = rowData.Substring(0, idx - 1).Replace("\"", "");
                        string v = rowData.Substring(idx + 1).Replace("\"", "");
                        nr[n] = v;
                    }
                    catch
                    {
                        continue;
                    }
                }
                dt.Rows.Add(nr);
            }
            return dt;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    #endregion
}
