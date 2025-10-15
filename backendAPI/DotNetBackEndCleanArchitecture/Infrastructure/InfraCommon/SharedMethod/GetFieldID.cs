using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraCommon.SharedMethod
{
    public static class GetFieldID
    {
        public static string process(string id, long sequence,DateTime timeStamp)
        {
            string mCombinedValue = string.Empty;
            try
            {
                string mId = id;//如果是交易紀錄的自製ID就是tr
                string mTimeStamp = timeStamp.ToString("yyyyMMdd");

                mCombinedValue = $@"{mId}_{mTimeStamp}_{sequence.ToString("D10")}";
            }
            catch (Exception)
            {
                throw;
            }

            return mCombinedValue;
        }
    }
}
