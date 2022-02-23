using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Signer;

namespace Opkee.Token
{
    public class Tool
    {
        static public string RootDirectory = Environment.CurrentDirectory;

        public static string GetSenderAddress(string plainMessage, string signature)
        {
            EthereumMessageSigner messageSigner = new EthereumMessageSigner();
            return messageSigner.EncodeUTF8AndEcRecover(plainMessage, signature);
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}