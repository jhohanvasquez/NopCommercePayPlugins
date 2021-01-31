using System;
using Nop.Core.Configuration;
using System.Security.Cryptography;
using System.Text;
using Nop.Plugin.Payments.PayUColombia.Enumerator;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nop.Plugin.Payments.PayUColombia
{
    public class PayUColombiaHelper
    {       

        public string Getchecksum(string apiKey, string merchantId, string referenceCode, string tx_value, string currency)
        {
            string checksumString;
           
            checksumString = apiKey + "~" + merchantId + "~" + referenceCode + "~" + tx_value + "~" + currency;
            
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
        public static PaymentStatus GetPaymentStatus(string paymentStatus, string pendingReason)
        {
            var result = PaymentStatus.Pending;

            if (paymentStatus == null)
                paymentStatus = string.Empty;

            if (pendingReason == null)
                pendingReason = string.Empty;

            switch (paymentStatus.ToLowerInvariant())
            {
                case "approved":
                    result = PaymentStatus.Approved;
                    break;
                case "pending":                   
                    result = PaymentStatus.Pending;                         
                    break;               
                case "expired":
                    result = PaymentStatus.Expired;
                    break;
                case "error":
                    result = PaymentStatus.Error;
                    break;
                case "declined":
                    result = PaymentStatus.Declined;
                    break;
                default:
                    break;
            }

            return result;
        }


    }	
}
