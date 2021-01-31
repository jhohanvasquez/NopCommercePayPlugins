using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;


namespace Nop.Plugin.Payments.PayZonaPagos
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {

            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayZonaPagos.Configure",
                 "Plugins/PaymentPayZonaPagos/Configure",
                 new { controller = "PaymentPayZonaPagos", action = "Configure" });

            //routeBuilder.MapRoute("Plugin.Payments.PayZonaPagos.Configure", "Plugins/Payments.PayZonaPagos/Views/Configure",
            //     new { controller = "PaymentPayZonaPagos", action = "Configure" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayZonaPagos.PaymentInfo",
                 "Plugins/PaymentPayZonaPagos/PaymentInfo",
                 new { controller = "PaymentPayZonaPagos", action = "PaymentInfo" });



            //Return
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayZonaPagos.Return",
                 "Plugins/PaymentPayZonaPagos/Return",
                 new { controller = "PaymentPayZonaPagos", action = "Return" });

    
        }
        public int Priority
        {
            get
            {
                return -1;
            }
        }
    }
}
