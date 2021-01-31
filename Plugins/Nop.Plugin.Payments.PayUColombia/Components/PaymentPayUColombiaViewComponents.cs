using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.PayUColombia.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.PayUColombia.Components
{
    [ViewComponent(Name = "PaymentPayUColombia")]
    public class PaymentPayUColombiaViewComponents : NopViewComponent
    {

        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel();
            return View("~/Plugins/Payments.PayUColombia/Views/PaymentInfo.cshtml",model);
        }
    }
}
