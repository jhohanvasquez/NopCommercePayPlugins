using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.PayEpayco.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PayEpayco.Components
{
    [ViewComponent(Name = "PaymentPayEpayco")]
    public class PaymentPayEpaycoViewComponents : NopViewComponent
    {

        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();
            return View("~/Plugins/Payments.PayEpayco/Views/PaymentInfo.cshtml",model);
        }
    }
}
