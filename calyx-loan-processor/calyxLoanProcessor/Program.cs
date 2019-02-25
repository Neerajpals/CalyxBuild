using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Data;
using Calyx.Point.Data.Common;
using Calyx.Point.Data;
using Calyx.Point.SDK.Results;
using Calyx.Point;
using Calyx.Point.SDK;
using Calyx.Point.Data.DataFolderServices;
using Calyx.Point.Data.Specialized;
using calyxLoanProcessor.Model;
using Newtonsoft.Json;

namespace CalyxLoanProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string commandType = args[0];
                switch (commandType)
                {
                    case "GET":
                        {

                            string userID = args[1];
                            string password = args[2];
                            string loanID = args[3];

                            GetLoan(userID, password, loanID);
                            break;
                        }

                    case "PUT":
                        {
                            string json = args[1];

                            UpdateLoanRequest request = JsonConvert.DeserializeObject<UpdateLoanRequest>(json);
                            PutLoan(request);
                            break;
                        }

                    case "POST":
                        {
                            string json = args[1];

                            PostNewLoanRequest request = JsonConvert.DeserializeObject<PostNewLoanRequest>(json);
                            PostLoan(request);
                            break;
                        }
                    default:
                        {
                            OutputResult($"Command Type : { args[0] } is invalid.");
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                OutputResult(ex.ToString());
            }
        }

        private static PointUserLoginResults CreateSession(string userID, string password)
        {
            try
            {
                PointSDK pointSDK = PointSDK.GetPDK();
                PointUserLoginResults loginResult = pointSDK.ClientLogin(userID, password);
                return loginResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void CloseSession()
        {
            try
            {
                PointSDK.GetPDK().ClientLogout();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void PostLoan(PostNewLoanRequest request)
        {
            try
            {
                PointUserLoginResults loginResult = CreateSession(request.userName, request.password);

                string loanFileName = Guid.NewGuid().ToString();

                DataFolderInfo dataFolder = loginResult.UserInfo.DataFolders.FirstOrDefault();
                if (dataFolder != null)
                {
                    LoanFile loanFile = LoanInfo.Create(Calyx.Point.Data.Common.LoanType.Borrower);
                    foreach (LoanField loanField in request.loanFields)
                    {
                        Bind_FieldID_Name();
                        int fieldID = GetFieldID(loanField.fieldName.ToLower().Trim());
                        if (fieldID > 0)
                        {
                            BorrowerSetPositionType positionType = (BorrowerSetPositionType)loanField.borrowerPosition;
                            loanFile.SetData(fieldID, BorrowerSetPositionType.Borrower, loanField.fieldValue);
                        }
                        else
                        {
                            OutputResult($"The field name : { loanField.fieldName } is invalid.");
                            return;
                        }
                    }

                    loanFile.Save(loanFileName, dataFolder);
                    loanFile.Close();
                    OutputResult($"Loan Created Successfully With LoanFile Name : { loanFile.Info.Attributes.FileName }");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseSession();
            }
        }

        private static LoanInfo GetLoanInfo(PointUserLoginResults loginResult, string loanFileName)
        {
            LoanInfo loanInfo = null;

            DataFolderInfo dataFolder = loginResult.UserInfo.DataFolders.FirstOrDefault();

            GetLoanResults loanResults = dataFolder.GetLoans(SearchLoanType.Borrower, SearchByType.FileName, Calyx.Point.Data.DataFolderServices.SearchOption.MatchesWith, loanFileName, "");

            foreach (var loan in loanResults.Loans)
            {
                loanInfo = loan;
            }

            return loanInfo;
        }

        private static void GetLoan(string userID, string password, string loanID)
        {
            try
            {
                PointUserLoginResults loginResult = CreateSession(userID, password);
                LoanInfo loanInfo = GetLoanInfo(loginResult, loanID);
                if (loanInfo != null)
                {
                    LoanFile loanFile = loanInfo.Open(true);
                    XmlDocument xmlResult = loanFile.ToMismo();
                    loanFile.Close();

                    string result = "";
                    using (var stringWriter = new StringWriter())
                    {
                        using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                        {
                            xmlResult.WriteTo(xmlTextWriter);
                            xmlTextWriter.Flush();
                            result = stringWriter.GetStringBuilder().ToString();
                        }
                    }
                    OutputResult(result);
                }
                else
                {
                    string message = $"loanID : { loanID } is invalid or doesn't exists.";
                    OutputResult(message);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseSession();
            }
        }

        private static void PutLoan(UpdateLoanRequest request)
        {
            try
            {
                PointUserLoginResults loginResult = CreateSession(request.userName, request.password);
                LoanInfo loanInfo = GetLoanInfo(loginResult, request.loanID);
                if (loanInfo != null)
                {
                    LoanFile loanFile = loanInfo.Open(false);
                    foreach (LoanField loanField in request.loanFields)
                    {
                        Bind_FieldID_Name();
                        int fieldID = GetFieldID(loanField.fieldName.ToLower().Trim());
                        if (fieldID > 0)
                        {
                            BorrowerSetPositionType positionType = (BorrowerSetPositionType)loanField.borrowerPosition;

                            loanFile.SetData(fieldID, BorrowerSetPositionType.Borrower, loanField.fieldValue);
                        }
                        else
                        {
                            OutputResult($"The field name : { loanField.fieldName } is invalid.");
                            return;
                        }
                    }

                    loanFile.Save();
                    loanFile.Close();
                    OutputResult($"Loan Updated Successfully With LoanFile Name : { loanFile.Info.Attributes.FileName}");
                }
                else
                {
                    string message = $"loanID : { request.loanID } is invalid or doesn't exists.";
                    OutputResult(message);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseSession();
            }
        }

        private static Dictionary<string, int> Mapper = new Dictionary<string, int>();
        private static int GetFieldID(string fieldName)
        {
            int fieldID = 0;
            Mapper.TryGetValue(fieldName, out fieldID);
            return fieldID;
        }

        private static void Bind_FieldID_Name()
        {
            try
            {
                if (Mapper.Count > 0) Mapper.Clear();//refreshing Mapper Fields

                DataTable dt = DataLayer.dl.GetAllData("Bind_FieldID_Name");
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        Mapper.Add(dr["FieldName"].ToString().ToLower().Trim(), Convert.ToInt32(dr["FieldID"]));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void OutputResult(string result)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\ProcessOutput.txt";
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            else
            {
                //Flush Text File
                File.WriteAllText(path, "");
            }
            File.WriteAllText(path, result);
        }
    }
}
