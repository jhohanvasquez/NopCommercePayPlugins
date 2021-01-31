using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.PayZonaPagos.Models;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Localization;
using Nop.Plugin.Payments.PayZonaPagos.Enumerator;
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

namespace Nop.Plugin.Payments.PayZonaPagos.Controllers
{
    public class PaymentPayZonaPagosController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly INotificationService _notificationService;
        private readonly PayZonaPagosPaymentSettings _payZonaPagosPaymentSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger _logger;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IWebHelper _webHelper;

        public PaymentPayZonaPagosController(ISettingService settingService,
            IPaymentService paymentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService, INotificationService notificationService,
             ILocalizationService localizationService,
            PayZonaPagosPaymentSettings payuPaymentSettings,
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
            _payZonaPagosPaymentSettings = payuPaymentSettings;
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
            model.Uri = _payZonaPagosPaymentSettings.Uri;
            model.Nombre = _payZonaPagosPaymentSettings.Nombre;
            model.TipoSeguridad = _payZonaPagosPaymentSettings.TipoSeguridad;
            model.DirectorioVirtual = _payZonaPagosPaymentSettings.DirectorioVirtual;
            model.Token = _payZonaPagosPaymentSettings.Token;
            model.Usuario = _payZonaPagosPaymentSettings.Usuario;
            model.Clave = _payZonaPagosPaymentSettings.Clave;
            model.RutaToken = _payZonaPagosPaymentSettings.RutaToken;
            model.EncabezadoToken = _payZonaPagosPaymentSettings.EncabezadoToken;
            model.EncabezadoUsuario = _payZonaPagosPaymentSettings.EncabezadoUsuario;
            model.EncabezadoClave = _payZonaPagosPaymentSettings.EncabezadoClave;
            model.Prefijo = _payZonaPagosPaymentSettings.Prefijo;
            model.AdditionalFee = _payZonaPagosPaymentSettings.AdditionalFee;
            model.RutaMetodo = _payZonaPagosPaymentSettings.RutaMetodo;

            // return View("Nop.Plugin.Payments.PayZonaPagos.Views.PaymentPayZonaPagos.Configure", model);

            return View("~/Plugins/Payments.PayZonaPagos/Views/Configure.cshtml", model);

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
            _payZonaPagosPaymentSettings.Uri = model.Uri;
            _payZonaPagosPaymentSettings.Nombre = model.Nombre;
            _payZonaPagosPaymentSettings.TipoSeguridad = model.TipoSeguridad;
            _payZonaPagosPaymentSettings.DirectorioVirtual = model.DirectorioVirtual;
            _payZonaPagosPaymentSettings.Token = model.Token;
            _payZonaPagosPaymentSettings.Usuario = model.Usuario;
            _payZonaPagosPaymentSettings.Clave = model.Clave;
            _payZonaPagosPaymentSettings.RutaToken = model.RutaToken;
            _payZonaPagosPaymentSettings.EncabezadoToken = model.EncabezadoToken;
            _payZonaPagosPaymentSettings.EncabezadoUsuario = model.EncabezadoUsuario;
            _payZonaPagosPaymentSettings.DirectorioVirtual = model.DirectorioVirtual;
            _payZonaPagosPaymentSettings.EncabezadoClave = model.EncabezadoClave;
            _payZonaPagosPaymentSettings.Prefijo = model.Prefijo;
            _payZonaPagosPaymentSettings.RutaMetodo = model.RutaMetodo;
            _payZonaPagosPaymentSettings.AdditionalFee = model.AdditionalFee;

            _settingService.SaveSetting(_payZonaPagosPaymentSettings);

            //return View("Nop.Plugin.Payments.PayZonaPagos.Views.PaymentPayZonaPagos.Configure", model);
            //return View("~/Plugins/Payments.PayZonaPagos/Views/PaymentPayZonaPagos/Configure.cshtml", model);
            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        //[ChildActionOnly]
        public IActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            //return View("Nop.Plugin.Payments.PayZonaPagos.Views.PaymentPayZonaPagos.PaymentInfo", model);
            return View("~/Plugins/Payments.PayZonaPagos/Views/PaymentPayZonaPagos/PaymentInfo.cshtml", model);

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

            if (!(_paymentPluginManager.LoadPluginBySystemName("Payments.PayZonaPagos") is PayZonaPagosPaymentProcessor processor) || !_paymentPluginManager.IsPluginActive(processor))
                throw new NopException("ZonaPagos module cannot be loaded");

            var paymentStatus = _webHelper.QueryString<string>("estadoSolicitud");
            var transactionId = _webHelper.QueryString<string>("Id_pago");
            var orderNumber = _webHelper.QueryString<string>("idOrden");
            var paymentStatusName = string.Empty;

            switch (paymentStatus)
            {
                case "0":
                    paymentStatusName = "Aprobada";
                    break;
                case "1":
                    paymentStatusName = "Rechazada";
                    break;
                case "2":
                    paymentStatusName = "Pendiente";
                    break;
            }

            var sb = new StringBuilder();
            sb.AppendLine("PayU IPN:");            
            sb.AppendLine("estadoSolicitud" + ": " + paymentStatusName);
            sb.AppendLine("Id_pago" + ": " + transactionId);
            sb.AppendLine("idOrden" + ": " + orderNumber);
            var ipnInfo = sb.ToString();

            var order = _orderService.GetOrderById(Convert.ToInt32(orderNumber));

            if (order == null)
            {
                var errorStr = "ZonaPagos Order is not found";
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

            if (paymentStatus == "0")
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