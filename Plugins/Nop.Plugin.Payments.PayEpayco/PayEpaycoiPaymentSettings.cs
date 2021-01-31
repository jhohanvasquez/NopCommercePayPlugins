using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.PayEpayco
{
    public class PayEpaycoPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }
        public string AccountID { get; set; }
        public string Key { get; set; }
        public string Publickey { get; set; }
        public string PayEpaycoUri { get; set; }
        public decimal AdditionalFee { get; set; }       
    }
}
