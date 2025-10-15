namespace InfraCommon.SharedMethod
{
    public static class GetTimeZoneInfo
    {
        public static DateTime process()
        {
            DateTime mResult = DateTime.MinValue;
            try
            {
                // 假設現在是要從標準時區 +00:00 轉換到台灣時區，故這邊使用 UtcNow 先取標準世界協調時間
                var mNowDateTime = DateTime.UtcNow;

                // 使用 TimeZoneInfo 先取得台北時區
                var mTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Taipei");//Linux
                //var mTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Taipei Standard Time");//Window

                // 再使用 TimeZoneInfo 來變更時間
                //DateTime convertedDateTime = TimeZoneInfo.ConvertTime(nowDateTime, timeZone);
                mResult = TimeZoneInfo.ConvertTime(mNowDateTime, mTimeZone);

                //convertedDateTime.ToString("yyyy/MM/dd H:mm:ss zzz").Dump();
                // 2020/08/30 23:56:05 +08:00
                // 可以看到除了時間變更以外，時區也切換到 +08:00 了！
            }
            catch (Exception)
            {
                throw;
            }

            return mResult;
        }
    }
}
