using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
namespace Nop.Plugin.Payments.PayWompi.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }     

        [NopResourceDisplayName("Plugins.Payments.PayWompi.Publickey")]
        public string Publickey { get; set; }
        public bool Publickey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayWompi.PayWompiUri")]
        public string PayWompiUri { get; set; }
        public bool PayWompiUri_OverrideForStore { get; set; }       

        [NopResourceDisplayName("Plugins.Payments.PayWompi.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }
    }
}