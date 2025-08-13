using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace PropertyManagement.Core
{
    public class UserMebership
    {
        private static byte[] HexToByte(string hexString)
        {
            var returnBytes = new byte[hexString.Length / 2];
            for (var i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        public virtual string EncodeHashPassword(string validationKey, string password)
        {
            var hash = new HMACSHA1 { Key = HexToByte(validationKey) };
            return Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
        }
    }
}
