using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.PayUColombia
{
    public class PayUColombiaPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use sandbox (testing environment)
        /// </summary>
        public bool UseSandbox { get; set; }
        public string AccountID { get; set; }
        public string ApiLogin { get; set; }
        public string ApiKey { get; set; }
        public string MerchantID { get; set; }
        public string PayUColombiaUri { get; set; }
        public decimal AdditionalFee { get; set; }
    }
}
