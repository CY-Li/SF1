using System.Security.Cryptography;
using System.Text;

namespace InfraCommon.Common
{
    public class AesProcess : ICryptography
    {
        /// <summary>
        /// 加密後回傳base64String，相同明碼文字編碼後的base64String結果會相同(類似雜湊)，除非變更key或iv
        /// 如果key和iv忘記遺失的話，資料就解密不回來
        /// base64String若使用在Url的話，Web端記得做UrlEncode
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public string EncryptData(string data, string key = "1qaz2wsx3edc4rfv", string iv = "4rfv3edc2wsx1qaz")
        {
            ValidateKeyIVLength(key, iv);
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;//非必須，但加了較安全
            aes.Padding = PaddingMode.PKCS7;//非必須，但加了較安全

            ICryptoTransform transform = aes.CreateEncryptor(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv));

            byte[] bPlainText = Encoding.UTF8.GetBytes(data);//明碼文字轉byte[]
            byte[] outputData = transform.TransformFinalBlock(bPlainText, 0, bPlainText.Length);//加密
            return Convert.ToBase64String(outputData);
        }

        public string DecryptData(string base64String, string key = "1qaz2wsx3edc4rfv", string iv = "4rfv3edc2wsx1qaz")
        {
            ValidateKeyIVLength(key, iv);
            Aes aes = Aes.Create();
            aes.Mode = CipherMode.CBC;//非必須，但加了較安全
            aes.Padding = PaddingMode.PKCS7;//非必須，但加了較安全

            ICryptoTransform transform = aes.CreateDecryptor(Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(iv));
            byte[] bEnBase64String ;
            byte[] outputData;
            try
            {
                bEnBase64String = Convert.FromBase64String(base64String);//有可能base64String格式錯誤
                outputData = transform.TransformFinalBlock(bEnBase64String, 0, bEnBase64String.Length);//有可能解密出錯
            }
            catch (Exception ex)
            {
                //todo 寫Log
                throw new Exception($@"解密出錯:{ex.Message}");
            }

            //解密成功
            return Encoding.UTF8.GetString(outputData);
        }


        /// <summary>
        /// 驗證key和iv的長度(AES只有三種長度適用)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        private static void ValidateKeyIVLength(string key, string iv)
        {
            //驗證key和iv都必須為128bits或192bits或256bits
            List<int> LegalSizes = new List<int>() { 128, 192, 256 };
            int keyBitSize = Encoding.UTF8.GetBytes(key).Length * 8;
            int ivBitSize = Encoding.UTF8.GetBytes(iv).Length * 8;
            if (!LegalSizes.Contains(keyBitSize) || !LegalSizes.Contains(ivBitSize))
            {
                throw new Exception($@"key或iv的長度不在128bits、192bits、256bits其中一個，輸入的key bits:{keyBitSize},iv bits:{ivBitSize}");
            }
        }
    }
}
