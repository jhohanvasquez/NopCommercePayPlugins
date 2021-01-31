using System.ComponentModel.DataAnnotations;
using LoginApi;
using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.PayZonaPagos
{
    public class PayZonaPagosPaymentSettings : ISettings
    {
        
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Uri")]
        public string Uri { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Tipo de seguridad")]
        public TipoSeguridad TipoSeguridad { get; set; }

        [Display(Name = "Directorio virtual")]
        public string DirectorioVirtual { get; set; }
        public bool Visual { get; set; }

        [Display(Name = "Token Api")]
        public string Token { get; set; }

        [Display(Name = "Usuario Api")]
        public string Usuario { get; set; }

        [Display(Name = "Clave Api")]
        public string Clave { get; set; }

        [Display(Name = "Ruta token Api")]
        public string RutaToken { get; set; }


        [Display(Name = "Encabezado token Api")]
        public string EncabezadoToken { get; set; }

        [Display(Name = "Encabezado usuario Api")]
        public string EncabezadoUsuario { get; set; }

        [Display(Name = "Encabezado clave Api")]
        public string EncabezadoClave { get; set; }

        [Display(Name = "Prefijo url Api")]
        public string Prefijo { get; set; }

        [Display(Name = "Ruta Metodo de Pago")]
        public string RutaMetodo { get; set; }

        public decimal AdditionalFee { get; set; }
    }
}
