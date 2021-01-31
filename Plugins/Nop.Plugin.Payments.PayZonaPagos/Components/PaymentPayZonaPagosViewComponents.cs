using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.PayZonaPagos.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PayZonaPagos.Components
{
    [ViewComponent(Name = "PaymentPayZonaPagos")]
    public class PaymentPayZonaPagosViewComponents : NopViewComponent
    {

        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();
            return View("~/Plugins/Payments.PayZonaPagos/Views/PaymentInfo.cshtml",model);
        }
    }
}
