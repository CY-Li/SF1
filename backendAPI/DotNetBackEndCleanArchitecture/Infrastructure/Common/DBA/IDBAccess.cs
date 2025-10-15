namespace Common.DBA
{
    public interface IDBAccess
    {
        /// <summary>
        /// 設定連線字串
        /// </summary>
        /// <param name="p_strConnection">連線字串</param>
        void SetConnectionString(string p_strConnection);

        /// <summary>
        /// 取得資料
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_strSQL">TSQL 語法</param>
        /// <param name="p_strConnection">連線字串</param>
        /// <param name="p_parameters">參數(可NULL)</param>
        /// <param name="p_commandTimeout"></param>
        /// <returns></returns>
        List<T> GetObject<T>(string p_strSQL, object? p_parameters = null, int p_commandTimeout = 60);

        /// <summary>
        /// 執行新增、修改、刪除
        /// </summary>
        /// <param name="p_strSQL">TSQL</param>
        /// <param name="p_parameters">參數(不可NULL)</param>
        /// <param name="p_commandTimeout"></param>
        /// <returns></returns>
        int ExecNonQuery<T>(string p_strSQL, T p_parameters, int p_commandTimeout = 60);

        #region Transaction 會在Impl中包Try-Catch而不用全域方式做
        /// <summary>
        /// 開啟連線，設定Transaction
        /// </summary>
        void BeginTrans();
        /// <summary>
        /// 資料確認新增、修改、刪除，釋放資源
        /// </summary>
        void Commit();
        /// <summary>
        /// 資料取消新增、修改、刪除，釋放資源
        /// </summary>
        void Rollback();
        /// <summary>
        /// 執行寫入TSQL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_strSQL">TSQL</param>
        /// <param name="p_parameters">寫入資料</param>
        /// <param name="p_commandTimeout"></param>
        /// <returns></returns>
        int ExecuteTransactionObject<T>(string p_strSQL, T p_parameters, int p_commandTimeout = 60);
        /// <summary>
        /// 執行Transaction時，用來取得資料得Function
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p_strSQL">TSQL</param>
        /// <param name="parameters">取資料條件</param>
        /// <param name="p_commandTimeout"></param>
        /// <returns></returns>
        List<T> ExecuteTransactionGetObject<T>(string p_strSQL, object parameters, int p_commandTimeout = 60);
        #endregion END Transaction 會在Impl中包Try-Catch而不用全域方式做
    }
}
