using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;


namespace Nop.Plugin.Payments.PayUColombia
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {

            
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayUColombia.Configure",
                 "Plugins/PaymentPayUColombia/Configure",
                 new { controller = "PaymentPayUColombia", action = "Configure" });

            //endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayUColombia.Configure", "Plugins/Payments.PayUColombia/Views/Configure",
            //     new { controller = "PaymentPayUColombia", action = "Configure" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayUColombia.PaymentInfo",
                 "Plugins/PaymentPayUColombia/PaymentInfo",
                 new { controller = "PaymentPayUColombia", action = "PaymentInfo" });
            


            //Return
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.PayUColombia.Return",
                 "Plugins/PaymentPayUColombia/Return",
                 new { controller = "PaymentPayUColombia", action = "Return" });

    
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
