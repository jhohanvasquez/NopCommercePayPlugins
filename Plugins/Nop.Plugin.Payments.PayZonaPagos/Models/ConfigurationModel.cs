using LoginApi;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
namespace Nop.Plugin.Payments.PayZonaPagos.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayZonaPagos.Uri")]
        public string Uri { get; set; }
        public bool Uri_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayZonaPagos.Nombre")]
        public string Nombre { get; set; }
        public bool Nombre_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayZonaPagos.Token")]
        public string Token { get; set; }
        public bool Token_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayZonaPagos.Usuario")]
        public string Usuario { get; set; }
        public bool Usuario_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayZonaPagos.Clave")]
        public string Clave { get; set; }
        public bool Clave_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayZonaPagos.DirectorioVirtual")]
        public string DirectorioVirtual { get; set; }
        public bool DirectorioVirtual_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayZonaPagos.RutaToken")]
        public string RutaToken { get; set; }
        public bool RutaToken_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayZonaPagos.TipoSeguridad")]
        public TipoSeguridad TipoSeguridad { get; set; }
        public bool TipoSeguridad_OverrideForStore { get; set; }        

        [NopResourceDisplayName("Plugins.Payments.PayZonaPagos.EncabezadoToken")]
        public string EncabezadoToken { get; set; }
        public bool EncabezadoToken_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayZonaPagos.EncabezadoUsuario")]
        public string EncabezadoUsuario { get; set; }
        public bool EncabezadoUsuario_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayZonaPagos.EncabezadoClave")]
        public string EncabezadoClave { get; set; }
        public bool EncabezadoClave_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayZonaPagos.Prefijo")]
        public string Prefijo { get; set; }
        public bool Prefijo_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayZonaPagos.RutaMetodo")]
        public string RutaMetodo { get; set; }
        public bool RutaMetodo_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.PayZonaPagos.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }


    }
}