using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraCommon.Common
{
    public interface ICryptography
    {
        string EncryptData(string data, string key = "1qaz2wsx3edc4rfv", string iv = "4rfv3edc2wsx1qaz");

        string DecryptData(string data, string key = "1qaz2wsx3edc4rfv", string iv = "4rfv3edc2wsx1qaz");
    }
}
