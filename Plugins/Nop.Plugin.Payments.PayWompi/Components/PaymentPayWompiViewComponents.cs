using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.PayWompi.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PayWompi.Components
{
    [ViewComponent(Name = "PaymentPayWompi")]
    public class PaymentPayWompiViewComponents : NopViewComponent
    {

        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();
            return View("~/Plugins/Payments.PayWompi/Views/PaymentInfo.cshtml",model);
        }
    }
}
