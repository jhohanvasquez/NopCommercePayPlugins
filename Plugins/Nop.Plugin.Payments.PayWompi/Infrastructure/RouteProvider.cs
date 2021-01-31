using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;


namespace Nop.Plugin.Payments.PayWompi
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {


            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayWompi.Configure",
                 "Plugins/PaymentPayWompi/Configure",
                 new { controller = "PaymentPayWompi", action = "Configure" });

            //endpointRouteBuilder.MapRoute("Plugin.Payments.PayWompi.Configure", "Plugins/Payments.PayWompi/Views/Configure",
            //     new { controller = "PaymentPayWompi", action = "Configure" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayWompi.PaymentInfo",
                 "Plugins/PaymentPayWompi/PaymentInfo",
                 new { controller = "PaymentPayWompi", action = "PaymentInfo" });
            


            //Return
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayWompi.Return",
                 "Plugins/PaymentPayWompi/Return",
                 new { controller = "PaymentPayWompi", action = "Return" });

    
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
