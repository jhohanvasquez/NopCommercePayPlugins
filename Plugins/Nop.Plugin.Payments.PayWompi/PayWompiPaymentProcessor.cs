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

using Nop.Plugin.Payments.PayWompi.Controllers;
using Nop.Plugin.Payments.PayWompi.Services;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.PayWompi
{
    /// <summary>
    /// PayWompi payment processor
    /// </summary>
    public class PayWompiPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly PayWompiPaymentSettings _payWompiPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly WompiHttpClient _wompiHttpClient;
        #endregion

        #region Ctor

        public PayWompiPaymentProcessor(PayWompiPaymentSettings payWompiPaymentSettings,
            ISettingService settingService, ICurrencyService currencyService,
              ILocalizationService localizationService,
              IHttpContextAccessor httpContextAccessor,
            CurrencySettings currencySettings, IWebHelper webHelper, WompiHttpClient wompiHttpClient)
        {
             _localizationService = localizationService;
             _payWompiPaymentSettings = payWompiPaymentSettings;
             _settingService = settingService;
             _currencyService = currencyService;
             _currencySettings = currencySettings;
             _webHelper = webHelper;
            _httpContextAccessor = httpContextAccessor;
            _wompiHttpClient = wompiHttpClient;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods


        /// <summary>
        /// Create common query parameters for the request
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Created post parameters</returns>
        private Dictionary<string, string> CreatePostParameters(PostProcessPaymentRequest postProcessPaymentRequest)
        {

            var orderId = postProcessPaymentRequest.Order.Id;
            var remotePostHelper = new Dictionary<string, string>();
            var roundedOrderTotal = Convert.ToInt32(postProcessPaymentRequest.Order.OrderTotal);
            var roundedOrderTotalExcTaxSub = Convert.ToInt32(postProcessPaymentRequest.Order.OrderSubtotalExclTax);
            var roundedOrderTotalTax = Convert.ToInt32(postProcessPaymentRequest.Order.OrderTax);

            remotePostHelper.Add("public-key", _payWompiPaymentSettings.Publickey);
            remotePostHelper.Add("currency", _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode);
            remotePostHelper.Add("reference", orderId.ToString());
            remotePostHelper.Add("amount-in-cents", roundedOrderTotal.ToString());
            remotePostHelper.Add("redirect-url", _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayWompi/Return");
           
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
                FormName = "PayWompiForm",
                Url = _payWompiPaymentSettings.PayWompiUri,
                Method = "GET",
                AcceptCharset = "UTF-8"
            };
            foreach (var item in remotePostHelperData)
            {
                remotePostHelper.Add(item.Key, item.Value);
            }
            remotePostHelper.Post();

        }

        public bool GetTransactionDetails(string tx, out Dictionary<string, string> values, out string response)
        {
            var result = _wompiHttpClient.GetTransactionAsync(tx).Result;
            response = WebUtility.UrlDecode(result.Item2);
            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (result.Item1 == 200)
            {                
                foreach (var l in response.Split('\n'))
                {
                    var line = l.Trim();
                    var equalPox = line.IndexOf('=');
                    if (equalPox >= 0)
                        values.Add(line.Substring(0, equalPox), line.Substring(equalPox + 1));
                }
                return true;
            }
            else
            {               
                return false;
            }
            
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
            return _payWompiPaymentSettings.AdditionalFee;
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

            //PayWompi is the redirection payment method
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
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentPayWompi/Configure";
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
            return "PaymentPayWompi";
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
            controllerName = "PaymentPayWompi";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.PayWompi.Controllers" }, { "area", null } };
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
            controllerName = "PaymentPayWompi";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.PayWompi.Controllers" }, { "area", null } };
        }


        */
        public Type GetControllerType()
        {
            return typeof(PaymentPayWompiController);
        }

        public override void Install()
        {
            var settings = new PayWompiPaymentSettings()
            {               
                PayWompiUri = "https://checkout.wompi.co/p/",
                AdditionalFee = 0,
            };
            _settingService.SaveSetting(settings);

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayWompi.Publickey.Hint", "Ingrese su llave publica.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayWompi.Publickey", "Publickey");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayWompi.RedirectionTip", "Será redirigido al sitio de Wompi Colombia para completar el pedido.");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayWompi.PayWompiUri", "Pay URI");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayWompi.PayWompiUri.Hint", "Ingrese la Url de su plataforma de pagos Wompi.");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayWompi.AdditionalFee", "Tarifa Adicional");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayWompi.AdditionalFee.Hint", "Ingrese una tarifa adicional para cobrar a sus clientes.");

            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayWompi.Publickey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayWompi.Publickey");
            _localizationService. DeletePluginLocaleResource("Plugins.Payments.PayWompi.RedirectionTip");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayWompi.PayWompiUri");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayWompi.PayWompiUri.Hint");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayWompi.AdditionalFee");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayWompi.AdditionalFee.Hint");

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
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayWompi site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.PayWompi.RedirectionTip"); }
        }

        /*
        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "PaymentPayWompi";
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
