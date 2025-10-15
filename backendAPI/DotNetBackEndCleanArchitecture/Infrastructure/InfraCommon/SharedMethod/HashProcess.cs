namespace InfraCommon.SharedMethod
{
    public static class HashProcess
    {
        public static string GetHashPWD(string mm_pwd)
        {
            string mResult = string.Empty;

            if (!string.IsNullOrEmpty(mm_pwd) && 4 <= mm_pwd.Length && mm_pwd.Length <= 15)
            {
                var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
                //將密碼跟Salt雜湊演算
                mResult = BCrypt.Net.BCrypt.HashPassword(mm_pwd, salt);
            }
            return mResult;
        }

        public static bool VerifyHashPWD(string mm_pwd, string mm_hash_pwd)
        {
            bool mVerify = false;

            if (!string.IsNullOrEmpty(mm_hash_pwd)
                && !string.IsNullOrEmpty(mm_pwd)
                && 4 <= mm_pwd.Length
                && mm_pwd.Length <= 15)
            {
                mVerify = BCrypt.Net.BCrypt.Verify(mm_pwd, mm_hash_pwd);
            }

            return mVerify;
        }
    }
}
