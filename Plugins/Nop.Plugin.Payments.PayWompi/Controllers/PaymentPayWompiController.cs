using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.PayWompi.Models;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Localization;
using Nop.Plugin.Payments.PayWompi.Constans;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Nop.Services.Security;
using Nop.Core.Domain.Orders;
using System.Text;
using Nop.Services.Messages;
using Nop.Services.Logging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Nop.Plugin.Payments.PayWompi.Services;

namespace Nop.Plugin.Payments.PayWompi.Controllers
{
    public class PaymentPayWompiController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly INotificationService _notificationService;
        private readonly PayWompiPaymentSettings _wompiPaymentSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger _logger;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IWebHelper _webHelper;

        public PaymentPayWompiController(ISettingService settingService,
            IPaymentService paymentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService, INotificationService notificationService,
             ILocalizationService localizationService,
            PayWompiPaymentSettings wompiPaymentSettings,
            PaymentSettings paymentSettings,
            ILogger logger,
            IPaymentPluginManager paymentPluginManager,
            IPermissionService permissionService,IWebHelper webHelper)
        {
            _settingService = settingService;
            _paymentService = paymentService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _notificationService = notificationService;
            _wompiPaymentSettings = wompiPaymentSettings;
            _localizationService = localizationService;
            _paymentSettings = paymentSettings;
            _permissionService = permissionService;
            _logger = logger;
            _paymentPluginManager = paymentPluginManager;
            _webHelper = webHelper;
        }


        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            var model = new ConfigurationModel();
            model.Publickey = _wompiPaymentSettings.Publickey;
            model.PayWompiUri = _wompiPaymentSettings.PayWompiUri;
            model.AdditionalFee = _wompiPaymentSettings.AdditionalFee;

            // return View("Nop.Plugin.Payments.PayWompi.Views.PaymentPayWompi.Configure", model);

            return View("~/Plugins/Payments.PayWompi/Views/Configure.cshtml", model);

            //return View("~/Plugins/Payments.PayWompi/Views/PaymentPayWompi/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _wompiPaymentSettings.Publickey = model.Publickey;
            _wompiPaymentSettings.PayWompiUri = model.PayWompiUri;
            _wompiPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_wompiPaymentSettings);

            //return View("Nop.Plugin.Payments.PayWompi.Views.PaymentPayWompi.Configure", model);
            //return View("~/Plugins/Payments.PayWompi/Views/PaymentPayWompi/Configure.cshtml", model);
            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        //[ChildActionOnly]
        public IActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            //return View("Nop.Plugin.Payments.PayWompi.Views.PaymentPayWompi.PaymentInfo", model);
            return View("~/Plugins/Payments.PayWompi/Views/PaymentPayWompi/PaymentInfo.cshtml", model);

        }

        /*

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }
        */

       

        //[ValidateInput(false)]
        public IActionResult Return()
        {
            _webHelper.QueryString<string>("");

            if (!(_paymentPluginManager.LoadPluginBySystemName("Payments.PayWompi") is PayWompiPaymentProcessor processor) || !_paymentPluginManager.IsPluginActive(processor))
                throw new NopException("Wompi Colombia module cannot be loaded");

            var transactionId = _webHelper.QueryString<string>("id");

            if (processor.GetTransactionDetails(transactionId, out var values, out var response))
            {

                values.TryGetValue("id", out var transactionID);
                values.TryGetValue("reference", out var orderNumber);
                values.TryGetValue("customer_email", out var customer_email);
                values.TryGetValue("amount_in_cents", out var amount_in_cents);
                values.TryGetValue("created_at", out var created_at);
                values.TryGetValue("payment_method_type", out var payment_method_type);
                values.TryGetValue("status", out var status);

                var sb = new StringBuilder();
                sb.AppendLine("PayWonpi :");
                sb.AppendLine("transactionID" + ": " + transactionID);
                sb.AppendLine("customer_email" + ": " + orderNumber);
                sb.AppendLine("reference" + ": " + orderNumber);
                sb.AppendLine("amount_in_cents" + ": " + amount_in_cents);
                sb.AppendLine("created_at" + ": " + created_at);
                sb.AppendLine("payment_method_type" + ": " + payment_method_type);
                sb.AppendLine("status" + ": " + status);

                var ipnInfo = sb.ToString();

                var mcGross = 0;

                try
                {
                    mcGross = Convert.ToInt32(amount_in_cents);
                }
                catch (Exception exc)
                {
                    _logger.Error("EpayCo PDT. Error getting gross", exc);
                }

                var order = _orderService.GetOrderById(Convert.ToInt32(orderNumber));

                if (order == null)
                {
                    var errorStr = "Wompi Order is not found";
                    _logger.Error(errorStr, new NopException(ipnInfo));
                    return Content(errorStr);
                }
                //order note
                            _orderService.InsertOrderNote(new OrderNote
                {
                    Note = ipnInfo,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);

                //validate order total
                if (status == "APPROVED" && mcGross.Equals(order.OrderTotal))
                {
                    var errorStr = $"EpayCo Returned order total {mcGross} doesn't equal order total {order.OrderTotal}. Order# {order.Id}.";
                    //log
                    _logger.Error(errorStr);
                    //order note
                    _orderService.InsertOrderNote(new OrderNote
                    {
                        Note = errorStr,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);

                    return Content(errorStr);
                }
                if (status == "APPROVED")
                {
                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        order.AuthorizationTransactionId = transactionId;
                        _orderService.UpdateOrder(order);
                    }

                    //Thank you for shopping with us. Your credit card has been charged and your transaction is successful
                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                }
                else
                {
                    /*
                            Here you need to put in the routines for a failed
                            transaction such as sending an email to customer
                            setting database status etc etc
                        */

                    return RedirectToAction("Index", "Home", new { area = "" });

                }

            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }                
        }
    }
}