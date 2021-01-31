using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.PayWompi
{
    public class PayWompiPaymentSettings : ISettings
    {
        public string Publickey { get; set; }
        public string PayWompiUri { get; set; }
        public decimal AdditionalFee { get; set; }
    }
}
