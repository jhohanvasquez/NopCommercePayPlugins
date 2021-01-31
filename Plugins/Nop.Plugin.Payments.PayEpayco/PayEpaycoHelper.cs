using System;
using Nop.Core.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nop.Plugin.Payments.PayEpayco
{
    public class PayEpaycoHelper
    {       

        public string Getchecksum( string p_cust_id_cliente, string p_key, string p_id_invoice, string p_amount, string p_currency_code)
        {
            string checksumString;
           
            checksumString = p_cust_id_cliente + "^" + p_key + "^" + p_id_invoice + "^" + p_amount + "^" + p_currency_code;
            
            return ComputeMD5Hash(checksumString);
        }

        private string ComputeMD5Hash(string rawData)
        {
            var md5Hasher = MD5.Create();
            var bytes = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            // Create a SHA256   

            // Convert byte array to a string   
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        /// <summary>
        /// Gets a payment status
        /// </summary>
        /// <param name="paymentStatus">PayPal payment status</param>
        /// <param name="pendingReason">PayPal pending reason</param>
        /// <returns>Payment status</returns>
        public static string GetPaymentStatus(int paymentStatus)
        {
            string result = string.Empty;
            switch (paymentStatus)
            {
                case 1:
                    result = "Aceptada";
                    break;
                case 2:
                    result = "Rechazada";
                    break;
                case 3:
                    result = "Pendiente";
                    break;
                case 4:
                    result = "Fallida";
                    break;
            }

            return result;
        }

    }	
}
