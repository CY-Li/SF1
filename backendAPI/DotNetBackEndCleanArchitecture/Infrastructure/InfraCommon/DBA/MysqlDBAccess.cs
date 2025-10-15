using Dapper;
using InfraCommon.Common;
using MySqlConnector;

namespace InfraCommon.DBA
{
    public class MysqlDBAccess : IDBAccess
    {
        private string gStrConnection = string.Empty;
        //transaction 時使用
        private MySqlConnection? gConn;
        private MySqlTransaction? gTransaction;

        private readonly ICryptography _cryptography;

        public MysqlDBAccess(ICryptography cryptography)
        {
            _cryptography = cryptography;
        }

        /// <summary>
        /// 設定連線字串
        /// </summary>
        /// <param name="strConnection">連線自串</param>
        public void SetConnectionString(string strConnection)
        {
            // 檢查是否為明文連接字串（包含 Server= 關鍵字）
            if (strConnection.Contains("Server=", StringComparison.OrdinalIgnoreCase))
            {
                // 已經是明文連接字串，直接使用
                gStrConnection = strConnection;
            }
            else
            {
                // 加密的連接字串，需要解密
                gStrConnection = _cryptography.DecryptData(strConnection);
            }
        }

        /// <summary>
        /// 取得資料
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strSQL">TSQL 語法</param>
        /// <param name="strConnection">連線字串</param>
        /// <param name="parameters">參數(可NULL)</param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public List<T> GetObject<T>(string strSQL, object? parameters = null, int commandTimeout = 60)
        {
            List<T> mResult;

            using (MySqlConnection gConn = new MySqlConnection(gStrConnection))
            {
                gConn.Open();
                mResult = gConn.Query<T>(strSQL, parameters, null, true, commandTimeout).ToList();
            }
            return mResult;
        }

        //public List<T> GetObjectManyToMany<T,T2>(string p_strSQL, string[] p_corrParams, object? p_parameters = null, int p_commandTimeout = 60)
        //{
        //    List<T> m_result;

        //    using (SqlConnection g_conn = new SqlConnection(g_strConnection))
        //    {
        //        g_conn.Open();
        //        m_result = g_conn.Query<List<T>, T2, List<T>>(p_strSQL, (master, detail) =>
        //        {
        //            master.(typeof(T).GetProperty(p_corrParams[0]))
        //        }, p_parameters, null, true, p_commandTimeout).ToList();
        //    }
        //    return m_result;
        //}

        /// <summary>
        /// 執行新增、修改、刪除
        /// </summary>
        /// <param name="strSQL">TSQL</param>
        /// <param name="parameters">參數(不可NULL)</param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public int ExecNonQuery<T>(string strSQL, T parameters, int commandTimeout = 60)
        {
            int mResult = 0;
            using (MySqlConnection gConn = new MySqlConnection(gStrConnection))
            {
                gConn.Open();
                mResult = gConn.Execute(strSQL, parameters, null, commandTimeout);
            }
            return mResult;
        }

        #region Transaction
        /// <summary>
        /// 開啟連線，設定Transaction
        /// </summary>
        public void BeginTrans()
        {
            gConn = new MySqlConnection(gStrConnection);
            this.gConn.Open();
            this.gTransaction = gConn.BeginTransaction();
        }

        /// <summary>
        /// 資料確認新增、修改、刪除，釋放資源
        /// </summary>
        public void Commit()
        {
            if (gConn != null && gTransaction != null)
            {
                this.gTransaction.Commit();
                this.gConn.Close();
                this.gTransaction.Dispose();
            }

        }

        /// <summary>
        /// 資料取消新增、修改、刪除，釋放資源
        /// </summary>
        public void Rollback()
        {
            if (gConn != null && gTransaction != null)
            {
                this.gTransaction.Rollback();
                this.gConn.Close();
                this.gTransaction.Dispose();
            }
        }

        /// <summary>
        /// 執行寫入TSQL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strSQL">TSQL</param>
        /// <param name="parameters">寫入資料</param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public int ExecuteTransactionObject<T>(string strSQL, T parameters, int commandTimeout = 60)
        {
            int mResult = 0;

            if (gConn != null)
            {
                mResult = gConn.Execute(strSQL, parameters, gTransaction, commandTimeout);
            }

            return mResult;
        }

        /// <summary>
        /// 執行Transaction時，用來取得資料得Function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strSQL">TSQL</param>
        /// <param name="parameters">取資料條件</param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public List<T> ExecuteTransactionGetObject<T>(string strSQL, object parameters, int commandTimeout = 60)
        {
            List<T> mResult = new List<T>();

            if (gConn != null)
            {
                mResult = gConn.Query<T>(strSQL, parameters, gTransaction, true, commandTimeout).ToList();
            }

            return mResult;
        }
        #endregion END Transaction
    }
}
