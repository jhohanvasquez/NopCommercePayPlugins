using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
namespace Nop.Plugin.Payments.PayEpayco.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayEpayco.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayEpayco.AccountID")]
        public string AccountID { get; set; }
        public bool AccountID_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayEpayco.Key")]
        public string Key { get; set; }
        public bool Key_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayEpayco.Publickey")]
        public string Publickey { get; set; }
        public bool Publickey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayEpayco.PayEpaycoUri")]
        public string PayEpaycoUri { get; set; }
        public bool PayEpaycoUri_OverrideForStore { get; set; }       

        [NopResourceDisplayName("Plugins.Payments.PayEpayco.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }
    }
}