using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
namespace Nop.Plugin.Payments.PayUColombia.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayUColombia.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayUColombia.AccountID")]
        public string AccountID { get; set; }
        public bool AccountID_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayUColombia.ApiLogin")]
        public string ApiLogin { get; set; }
        public bool ApiLogin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayUColombia.ApiKey")]
        public string ApiKey { get; set; }
        public bool ApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayUColombia.MerchantID")]
        public string MerchantID { get; set; }
        public bool MerchantID_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayUColombia.PayUColombiaUri")]
        public string PayUColombiaUri { get; set; }
        public bool PayUColombiaUri_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayUColombia.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }
    }
}