using System;
using System.Collections.Generic;
using BEx.Framework.Base;
using System.Data.OleDb;
using System.Data;

namespace BEx.Framework.DataProvider
{
    public class Excel
    {
        private String ConnectionString { get; set; }
        private String SheetName { get; set; }
        private List<String> Headers { get; set; }
        private OleDbDataAdapter dataAdapter { get; set; }
        private DataSet excelDataSet { get; set; }
        public Dictionary<String, String> TestCaseData { get; set; } = new Dictionary<String, String>();

        #region OLEDB Methods
        public List<String> GetHeaders()
        {
            List<String> header = new List<String>();
            foreach (DataColumn m in excelDataSet.Tables[0].Columns)
                header.Add(m.ColumnName.ToString());
            this.Headers = header;
            return header;
        }
        public List<Dictionary<String, String>> GetData()
        {
            List<Dictionary<String, String>> dataList = new List<Dictionary<String, String>>();
            Dictionary<String, String> data = new Dictionary<String, String>();
            if (this.Headers == null)
                GetHeaders();
            for (int i = 0; i < excelDataSet.Tables[0].Rows.Count; i++)
            {
                for (Int32 j = 0; j < excelDataSet.Tables[0].Rows[i].ItemArray.Length; j++)
                    data.Add(Headers[j], excelDataSet.Tables[0].Rows[i].ItemArray[j].ToString());
                dataList.Add(data);
                data = new Dictionary<String, String>();
            }
            return dataList;
        }
        public String Get(String Key)
        {
            return TestCaseData[Key];
        }
        public Excel(String SheetName, String Mode = "Read")
        {
            ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;" +
                      "Data Source='" + BaseClass.DataFilePath +
                      "';Mode=" + Mode + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1;\"";
            this.SheetName = SheetName;
            using (OleDbConnection conn = new OleDbConnection(ConnectionString))
            {
                conn.Open();
                dataAdapter = new OleDbDataAdapter("select * from [" + SheetName + "$]", conn);
                excelDataSet = new DataSet();
                dataAdapter.Fill(excelDataSet);
                conn.Close();
            }
            if (this.Headers == null)
                GetHeaders();
        }
        /// <summary>
        /// Default contructor
        /// </summary>
        public Excel(String TestCaseName)
        {
            ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;" +
                      "Data Source='" + BaseClass.DataFilePath +
                      "';Mode=Read;Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1;\"";
            if (BaseClass.TestSuite[TestCaseName].TestSource.ToLower().Equals("coded"))
                GetCodedData();
        }
        private void GetCodedData()
        {
            using (OleDbConnection conn = new OleDbConnection(ConnectionString))
            {
                conn.Open();
                DataTable dataTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                var Sheet1 = dataTable.Rows[0].Field<string>("TABLE_NAME");

                dataAdapter = new OleDbDataAdapter("select * from [" + Sheet1 + "]", conn);
                excelDataSet = new DataSet();
                dataAdapter.Fill(excelDataSet);
                conn.Close();
            }
            if (this.Headers == null)
                GetHeaders();
            for (int i = 0; i < excelDataSet.Tables[0].Rows.Count; i++)
            {
                for (Int32 j = 0; j < excelDataSet.Tables[0].Rows[i].ItemArray.Length; j++)
                    TestCaseData.Add(Headers[j], excelDataSet.Tables[0].Rows[i].ItemArray[j].ToString());
            }
        }
        public Dictionary<String, String> GetCurrentTestCaseData()
        {
            return TestCaseData;
        }
        #endregion
    }
}
