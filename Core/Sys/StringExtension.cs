using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys
{
    public static class StringExtension
    {
        #region Hex <---> String

        /// <summary>
        /// Utility function:
        ///     conver string into byte array
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(String hexString)
        {
            int numberChars = hexString.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// Utility function:
        ///     convert byte array into string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            byte b;
            for (int i = 0; i < bytes.Length; ++i)
            {
                b = ((byte)(bytes[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);

                b = ((byte)(bytes[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }

            return new string(c);
        }


        #endregion

        #region String Encrypt/Decrypt

        public static string Encrypt(this string jsonString)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = encoding.GetBytes(jsonString);
            return ByteArrayToHexString(bytes);
        }

        public static string Decrypt(this string hexString)
        {
            byte[] bytes = HexStringToByteArray(hexString);
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            return encoding.GetString(bytes);
        }

        public static byte[] GetBytes(this string str)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            return encoding.GetBytes(str);
        }

        public static string GetString(this byte[] bytes)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            return encoding.GetString(bytes);
        }

        #endregion

        public static string ToSentence(this string sent)
        {
            return string.Join(" ", 
                sent.Trim().Split(new char[] { ' ' })
                .Where(word => word !="")
                .Select(word => word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower())
                );
        }
    }
}
