using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;


namespace Nop.Plugin.Payments.PayEpayco
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {


            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayEpayco.Configure",
                 "Plugins/PaymentPayEpayco/Configure",
                 new { controller = "PaymentPayEpayco", action = "Configure" });

            //endpointRouteBuilder.MapRoute("Plugin.Payments.PayEpayco.Configure", "Plugins/Payments.PayEpayco/Views/Configure",
            //     new { controller = "PaymentPayEpayco", action = "Configure" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayEpayco.PaymentInfo",
                 "Plugins/PaymentPayEpayco/PaymentInfo",
                 new { controller = "PaymentPayEpayco", action = "PaymentInfo" });
            


            //Return
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayEpayco.Return",
                 "Plugins/PaymentPayEpayco/Return",
                 new { controller = "PaymentPayEpayco", action = "Return" });

    
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
