
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class FileIO
{
    #region

    /// <summary>
    /// This function Returns Byte[] of the given file in Parameter
    /// </summary>
    /// <param name="filePath">Full Path of the File Specified</param>
    /// <returns></returns>
    public byte[] GetFileBytes(string filePath)
    {
        byte[] byteData = null;
        FileInfo fInfo = new FileInfo(filePath);
        long numBytes = fInfo.Length;
        //Open File in FileStream
        using (FileStream fStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            //Read filestream through BinaryReader
            using (BinaryReader br = new BinaryReader(fStream))
            {
                byteData = br.ReadBytes((int)numBytes);
            }
        }
        return byteData;
    }

    /// <summary>
    /// This function Save Byte[] into a file with Extension given in Parameter
    /// </summary>
    /// <param name="path">Path to Save File</param>
    /// <param name="extension">Extension of the File to Save(Without .)</param>
    /// <param name="byteFile">Byte[] of Files</param>
    /// <returns>FullPath of Stored File</returns>
    public string saveFile(string path, string extension, byte[] byteFile)
    {
        string fullPath = string.Format("{0}.{1}", path, extension);
        // write bytedate to disk as a file
        using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite))
        {
            // use a binary writer to write the bytes to disk
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(byteFile);
            }
        }
        return fullPath;
    }

    /// <summary>
    /// This function Save Byte[] into a temproryFile with Extension given in Parameter
    /// </summary>
    /// <param name="byteFile">Byte[] of Files</param>
    /// <param name="extension">Extension of the File to Save(Without .)</param>
    /// <returns>FullPath of temprory Stored File</returns>
    public string saveTempFile(byte[] byteFile, string extension)
    {
        //get temprory path(%temp%)
        string tempPath = Path.GetTempPath();
        //adding unique identifier name
        tempPath += Guid.NewGuid();
        //saving File and get fullPath
        string fullPath = saveFile(tempPath, extension, byteFile);
        return fullPath;
    }

    /// <summary>
    /// Get full temproryFile path with Extension given in Parameter
    /// </summary>
    /// <param name="extension">Extension of the File to Save(Without .)</param>
    /// /// <returns>Full Temprory Path</returns>
    public string getFullTempPath(string extension)
    {
        //get temprory path(%temp%)
        string fullpath = Path.GetTempPath();
        //adding unique identifier name
        fullpath += Guid.NewGuid();
        //adding extension
        fullpath += "." + extension;
        return fullpath;
    }

    #endregion
}
