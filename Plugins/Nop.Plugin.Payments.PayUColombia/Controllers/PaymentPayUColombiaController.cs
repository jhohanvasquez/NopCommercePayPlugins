using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.PayUColombia.Models;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Localization;
using Nop.Plugin.Payments.PayUColombia.Enumerator;
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

namespace Nop.Plugin.Payments.PayUColombia.Controllers
{
    public class PaymentPayUColombiaController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly INotificationService _notificationService;
        private readonly PayUColombiaPaymentSettings _payuPaymentSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger _logger;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IWebHelper _webHelper;

        public PaymentPayUColombiaController(ISettingService settingService,
            IPaymentService paymentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService, INotificationService notificationService,
             ILocalizationService localizationService,
            PayUColombiaPaymentSettings payuPaymentSettings,
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
            _payuPaymentSettings = payuPaymentSettings;
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
            model.UseSandbox = _payuPaymentSettings.UseSandbox;
            model.AccountID = _payuPaymentSettings.AccountID;
            model.ApiLogin = _payuPaymentSettings.ApiLogin;
            model.ApiKey = _payuPaymentSettings.ApiKey;
            model.MerchantID = _payuPaymentSettings.MerchantID;
            model.PayUColombiaUri = _payuPaymentSettings.PayUColombiaUri;
            model.AdditionalFee = _payuPaymentSettings.AdditionalFee;

            // return View("Nop.Plugin.Payments.PayUColombia.Views.PaymentPayUColombia.Configure", model);

            return View("~/Plugins/Payments.PayUColombia/Views/Configure.cshtml", model);

            //return View("~/Plugins/Payments.PayPalStandard/Views/PaymentPayPalStandard/Configure.cshtml", model);
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
            _payuPaymentSettings.UseSandbox = model.UseSandbox;
            _payuPaymentSettings.AccountID = model.AccountID;
            _payuPaymentSettings.ApiLogin = model.ApiLogin;
            _payuPaymentSettings.ApiKey = model.ApiKey;
            _payuPaymentSettings.MerchantID = model.MerchantID;
            _payuPaymentSettings.PayUColombiaUri = model.PayUColombiaUri;
            _payuPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_payuPaymentSettings);

            //return View("Nop.Plugin.Payments.PayUColombia.Views.PaymentPayUColombia.Configure", model);
            //return View("~/Plugins/Payments.PayUColombia/Views/PaymentPayUColombia/Configure.cshtml", model);
            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        //[ChildActionOnly]
        public IActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            //return View("Nop.Plugin.Payments.PayUColombia.Views.PaymentPayUColombia.PaymentInfo", model);
            return View("~/Plugins/Payments.PayUColombia/Views/PaymentPayUColombia/PaymentInfo.cshtml", model);

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

            if (!(_paymentPluginManager.LoadPluginBySystemName("Payments.PayUColombia") is PayUColombiaPaymentProcessor processor) || !_paymentPluginManager.IsPluginActive(processor))
                throw new NopException("PayU Colombia module cannot be loaded");

            var paymentStatus = _webHelper.QueryString<string>("lapTransactionState");
            var pendingReason = _webHelper.QueryString<string>("lapResponseCode");
            var transactionId = _webHelper.QueryString<string>("transactionId");
            var valueTotal = _webHelper.QueryString<int>("TX_VALUE");
            var orderNumber = _webHelper.QueryString<string>("referenceCode");


            var sb = new StringBuilder();
            sb.AppendLine("PayU IPN:");            
            sb.AppendLine("TransactionState" + ": " + paymentStatus);
            sb.AppendLine("ResponseCode" + ": " + pendingReason);
            sb.AppendLine("transactionId" + ": " + transactionId);
            var newPaymentStatus = PayUColombiaHelper.GetPaymentStatus(paymentStatus, pendingReason);
            sb.AppendLine("New payment status: " + newPaymentStatus);           
            var ipnInfo = sb.ToString();

            var mcGross = 0;

            try
            {
                mcGross = valueTotal;
            }
            catch (Exception exc)
            {
                _logger.Error("PayU PDT. Error getting gross", exc);
            }  

            var order = _orderService.GetOrderById(Convert.ToInt32(orderNumber));

            if (order == null)
            {
                var errorStr = "PayU Order is not found";
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
            if ((newPaymentStatus == Enumerator.PaymentStatus.Approved) && mcGross.Equals(order.OrderTotal))
            {
                var errorStr = $"PayU Returned order total {mcGross} doesn't equal order total {order.OrderTotal}. Order# {order.Id}.";
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

            if (newPaymentStatus == Enumerator.PaymentStatus.Approved)
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
    }
}