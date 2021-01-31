using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;

using Nop.Plugin.Payments.PayEpayco.Controllers;
using Nop.Plugin.Payments.PayEpayco.Services;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework;
using Nop.Plugin.Payments.PayEpayco;
using Nop.Services.Common;

namespace Nop.Plugin.Payments.PayEpayco
{
    /// <summary>
    /// PayEpayco payment processor
    /// </summary>
    public class PayEpaycoPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly PayEpaycoPaymentSettings _PayEpaycoPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly EpaycoHttpClient _EpaycoHttpClient;
        private readonly IAddressService _addressService;

        #endregion

        #region Ctor

        public PayEpaycoPaymentProcessor(PayEpaycoPaymentSettings PayEpaycoPaymentSettings,
            IAddressService addressService,
            ISettingService settingService, ICurrencyService currencyService,
              ILocalizationService localizationService,
              IHttpContextAccessor httpContextAccessor,
            CurrencySettings currencySettings, IWebHelper webHelper, EpaycoHttpClient EpaycoHttpClient)
        {
            _localizationService = localizationService;
            _PayEpaycoPaymentSettings = PayEpaycoPaymentSettings;
            _settingService = settingService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _webHelper = webHelper;
            _httpContextAccessor = httpContextAccessor;
            _EpaycoHttpClient = EpaycoHttpClient;
            _addressService = addressService;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        public bool GetTransactionDetails(string tx, out string response)
        {
            var result = _EpaycoHttpClient.GetTransactionAsync(tx).Result;
            response = WebUtility.UrlDecode(result.Item2);           
            if (result.Item1 == 200)
            {               
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Verifies IPN
        /// </summary>
        /// <param name="formString">Form string</param>
        /// <param name="values">Values</param>
        /// <returns>Result</returns>
        public bool GetParametersPost(string formString, out Dictionary<string, string> values)
        {
            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var l in formString.Split('&'))
            {
                var line = l.Trim();
                var equalPox = line.IndexOf('=');
                if (equalPox >= 0)
                    values.Add(line.Substring(0, equalPox), line.Substring(equalPox + 1));
            }
            return true;
        }


        /// <summary>
        /// Create common query parameters for the request
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Created post parameters</returns>
        private Dictionary<string, string> CreatePostParameters(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var myUtility = new PayEpaycoHelper();
            var orderId = postProcessPaymentRequest.Order.Id;
            var remotePostHelper = new Dictionary<string, string>();
            var roundedOrderTotal = Convert.ToInt32(postProcessPaymentRequest.Order.OrderTotal);
            var roundedOrderTotalExcTaxSub = Convert.ToInt32(postProcessPaymentRequest.Order.OrderSubtotalExclTax);
            var roundedOrderTotalTax = Convert.ToInt32(postProcessPaymentRequest.Order.OrderTax);


            remotePostHelper.Add("p_description", "Compra Epayco - NopEcommerce");
            remotePostHelper.Add("p_cust_id_cliente", _PayEpaycoPaymentSettings.AccountID);
            remotePostHelper.Add("p_key", _PayEpaycoPaymentSettings.Key);
            remotePostHelper.Add("p_tax", roundedOrderTotalTax.ToString());
            remotePostHelper.Add("p_amount_base", roundedOrderTotalExcTaxSub.ToString());
            remotePostHelper.Add("p_currency_code", _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode);
            remotePostHelper.Add("p_id_invoice", orderId.ToString());
            remotePostHelper.Add("p_amount", roundedOrderTotal.ToString());
            remotePostHelper.Add("p_url_response", _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayEpayco/Return");
            remotePostHelper.Add("p_test_request", _PayEpaycoPaymentSettings.UseSandbox ? "TRUE" : "FALSE");
            remotePostHelper.Add("p_signature", myUtility.Getchecksum(_PayEpaycoPaymentSettings.AccountID, _PayEpaycoPaymentSettings.Key, orderId.ToString(), roundedOrderTotal.ToString(), _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode));

            //choosing correct order address
            var orderAddress = _addressService.GetAddressById(
                 (postProcessPaymentRequest.Order.PickupInStore ? postProcessPaymentRequest.Order.PickupAddressId : postProcessPaymentRequest.Order.ShippingAddressId) ?? 0);

            remotePostHelper.Add("p_billing_address", orderAddress?.Address1);
            remotePostHelper.Add("p_billing_country", orderAddress?.County);


            if (postProcessPaymentRequest.Order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                remotePostHelper.Add("p_billing_name", orderAddress?.FirstName);
                remotePostHelper.Add("p_billing_lastname", orderAddress?.LastName);
                remotePostHelper.Add("p_billing_email", orderAddress?.Email);
                remotePostHelper.Add("p_billing_phone", orderAddress?.PhoneNumber);

            }

            return remotePostHelper;
        }


        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            return new ProcessPaymentResult();
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //create post
            var remotePostHelperData = CreatePostParameters(postProcessPaymentRequest);
            var remotePostHelper = new RemotePost
            {
                FormName = "PayEpaycoForm",
                Url = _PayEpaycoPaymentSettings.PayEpaycoUri,
                Method = "POST",
                AcceptCharset = "UTF-8"
            };
            foreach (var item in remotePostHelperData)
            {
                remotePostHelper.Add(item.Key, item.Value);
            }
            remotePostHelper.Post();

        }

        //Hide payment begins

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        //hide payment ends

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _PayEpaycoPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //PayEpayco is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice

            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1)
                return false;

            return true;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentPayEpayco/Configure";
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        public string GetPublicViewComponentName()
        {
            return "PaymentPayEpayco";
        }

        /*
       
        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentPayEpayco";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.PayEpayco.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentPayEpayco";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.PayEpayco.Controllers" }, { "area", null } };
        }


        */
        public Type GetControllerType()
        {
            return typeof(PaymentPayEpaycoController);
        }

        public override void Install()
        {
            var settings = new PayEpaycoPaymentSettings()
            {
                PayEpaycoUri = "https://secure.payco.co/checkout.php",
                AdditionalFee = 0,
            };
            _settingService.SaveSetting(settings);

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEpayco.AccountID.Hint", "Ingrese su AccountID de su cuenta de Epayco.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEpayco.AccountID", "AccountID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEpayco.Key.Hint", "Ingrese su llave Secreta.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEpayco.Key", "Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEpayco.Publickey.Hint", "Ingrese su llave publica.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEpayco.Publickey", "Publickey");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEpayco.RedirectionTip", "Será redirigido al sitio de Epayco Colombia para completar el pedido.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEpayco.PayEpaycoUri", "Pay URI");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEpayco.PayEpaycoUri.Hint", "Ingrese la Url de su plataforma de pagos Epayco.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEpayco.AdditionalFee", "Tarifa Adicional");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEpayco.AdditionalFee.Hint", "Ingrese una tarifa adicional para cobrar a sus clientes.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEpayco.UseSandbox", "Use Sandbox");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayEpayco.UseSandbox.Hint", "Verificar habilitar Sandbox (Escenario de pruebas).");

            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayEpayco.AccountID.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayEpayco.AccountID");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayEpayco.Key.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayEpayco.Key");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayEpayco.Publickey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayEpayco.Publickey");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayEpayco.RedirectionTip");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayEpayco.PayEpaycoUri");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayEpayco.PayEpaycoUri.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayEpayco.AdditionalFee");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayEpayco.AdditionalFee.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayEpayco.UseSandbox");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayEpayco.UseSandbox.Hint");


            base.Uninstall();
        }
        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;



        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayEpayco site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.PayEpayco.RedirectionTip"); }
        }

        /*
        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "PaymentPayEpayco";
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

    */
        #endregion
    }
}
