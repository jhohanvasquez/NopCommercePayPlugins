using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.PayEpayco.Models;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Localization;
using Nop.Plugin.Payments.PayEpayco.Constans;
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
using Nop.Plugin.Payments.PayEpayco.Services;
using LinqToDB.Common;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nop.Plugin.Payments.PayEpayco.Controllers
{
    public class PaymentPayEpaycoController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly INotificationService _notificationService;
        private readonly PayEpaycoPaymentSettings _EpaycoPaymentSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger _logger;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IWebHelper _webHelper;

        public PaymentPayEpaycoController(ISettingService settingService,
            IPaymentService paymentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService, INotificationService notificationService,
             ILocalizationService localizationService,
            PayEpaycoPaymentSettings EpaycoPaymentSettings,
            PaymentSettings paymentSettings,
            ILogger logger,
            IPaymentPluginManager paymentPluginManager,
            IPermissionService permissionService, IWebHelper webHelper)
        {
            _settingService = settingService;
            _paymentService = paymentService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _notificationService = notificationService;
            _EpaycoPaymentSettings = EpaycoPaymentSettings;
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
            model.UseSandbox = _EpaycoPaymentSettings.UseSandbox;
            model.AccountID = _EpaycoPaymentSettings.AccountID;
            model.Key = _EpaycoPaymentSettings.Key;
            model.Publickey = _EpaycoPaymentSettings.Publickey;
            model.PayEpaycoUri = _EpaycoPaymentSettings.PayEpaycoUri;
            model.AdditionalFee = _EpaycoPaymentSettings.AdditionalFee;

            // return View("Nop.Plugin.Payments.PayEpayco.Views.PaymentPayEpayco.Configure", model);

            return View("~/Plugins/Payments.PayEpayco/Views/Configure.cshtml", model);

            //return View("~/Plugins/Payments.PayEpayco/Views/PaymentPayEpayco/Configure.cshtml", model);
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
            _EpaycoPaymentSettings.UseSandbox = model.UseSandbox;
            _EpaycoPaymentSettings.AccountID = model.AccountID;
            _EpaycoPaymentSettings.Key = model.Key;
            _EpaycoPaymentSettings.Publickey = model.Publickey;
            _EpaycoPaymentSettings.PayEpaycoUri = model.PayEpaycoUri;
            _EpaycoPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_EpaycoPaymentSettings);

            //return View("Nop.Plugin.Payments.PayEpayco.Views.PaymentPayEpayco.Configure", model);
            //return View("~/Plugins/Payments.PayEpayco/Views/PaymentPayEpayco/Configure.cshtml", model);
            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        //[ChildActionOnly]
        public IActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            //return View("Nop.Plugin.Payments.PayEpayco.Views.PaymentPayEpayco.PaymentInfo", model);
            return View("~/Plugins/Payments.PayEpayco/Views/PaymentPayEpayco/PaymentInfo.cshtml", model);

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

            if (!(_paymentPluginManager.LoadPluginBySystemName("Payments.PayEpayco") is PayEpaycoPaymentProcessor processor) || !_paymentPluginManager.IsPluginActive(processor))
                throw new NopException("Epayco Colombia module cannot be loaded");


            var transactionId = _webHelper.QueryString<string>("ref_payco");

            if (processor.GetTransactionDetails(transactionId, out var response))
            {
                dynamic item = JObject.Parse(response);

                var transactionID  = (string)item["data"]["x_ref_payco"];
                var orderNumber = (string)item["data"]["x_id_factura"];
                var customer_email = (string)item["data"]["x_customer_email"];
                var amount_in_cents = (string)item["data"]["x_amount"];
                var created_at = (string)item["data"]["x_fecha_transaccion"];
                var status = (string)item["data"]["x_respuesta"];
                var reason_text = (string)item["data"]["x_response_reason_text"];

                var sb = new StringBuilder();
                sb.AppendLine("PayEpayCo :");
                sb.AppendLine("transactionID" + ": " + transactionID);
                sb.AppendLine("customer_email" + ": " + customer_email);
                sb.AppendLine("reference" + ": " + orderNumber);
                sb.AppendLine("amount" + ": " + amount_in_cents);
                sb.AppendLine("created_at" + ": " + created_at);
                sb.AppendLine("status" + ": " + status);
                sb.AppendLine("reason text" + ": " + reason_text);

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
                    var errorStr = "EpayCo Order is not found";
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
                if (status == "Aceptada" && mcGross.Equals(order.OrderTotal))
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
                if (status == "Aceptada")
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