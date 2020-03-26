using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackBlazeSDK
{
   public  class Utilitiez
    {

        public static string AsQueryString(Dictionary<string, string> parameters)
        {
            if (!parameters.Any()) { return string.Empty; }

            var builder = new StringBuilder("?");
            var separator = string.Empty;
            foreach (var kvp in parameters.Where(P => !string.IsNullOrEmpty(P.Value)))
            {
                builder.AppendFormat("{0}{1}={2}", separator, System.Net.WebUtility.UrlEncode(kvp.Key), System.Net.WebUtility.UrlEncode(kvp.Value.ToString()));
                separator = "&";
            }
            return builder.ToString();
        }

        public static string SHA1FileHash(byte[] BArray)
        {
            using (System.IO.BufferedStream bs = new System.IO.BufferedStream(new System.IO.MemoryStream(BArray)))
            {
                using (System.Security.Cryptography.SHA1Managed sha1 = new System.Security.Cryptography.SHA1Managed())
                {
                     byte[] hash = sha1.ComputeHash(bs);
                    StringBuilder formatted = new StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                    {
                        formatted.AppendFormat("{0:X2}", b);
                    }
                    return formatted.ToString();
                }
            }
        }

        public static byte[] StreamToByteArray(System.IO.Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length - 1 + 1];
            int totalBytesCopied = 0;

            while (totalBytesCopied < stream.Length)
                totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);

            return buffer;
        }

        public static string FileHash(byte[] BArray)
        {
            using (System.IO.BufferedStream bs = new System.IO.BufferedStream(new System.IO.MemoryStream(BArray)))
            {
                using (System.Security.Cryptography.SHA1Managed sha1 = new System.Security.Cryptography.SHA1Managed())
                {
                    byte[] hash = sha1.ComputeHash(bs);
                    System.Text.StringBuilder formatted = new System.Text.StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                        formatted.AppendFormat("{0:X2}", b);
                    return formatted.ToString();
                }
            }
        }

        public enum BucketTypesEnum
        {
            all,
            allPublic,
            allPrivate,
            snapshot
        }
        public enum UploadTypes
        {
            FilePath,
            Stream,
            BytesArry
        }




    }
}
