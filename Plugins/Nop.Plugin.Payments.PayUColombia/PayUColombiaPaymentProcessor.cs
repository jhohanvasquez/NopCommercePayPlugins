using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

using Nop.Plugin.Payments.PayUColombia.Controllers;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.PayUColombia
{
    /// <summary>
    /// PayUColombia payment processor
    /// </summary>
    public class PayUColombiaPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly PayUColombiaPaymentSettings _payUColombiaPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAddressService _addressService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public PayUColombiaPaymentProcessor(PayUColombiaPaymentSettings payUColombiaPaymentSettings,
            IAddressService addressService,
            ISettingService settingService, ICurrencyService currencyService,
              ILocalizationService localizationService,
              IHttpContextAccessor httpContextAccessor,
            CurrencySettings currencySettings, IWebHelper webHelper)
        {
             _localizationService = localizationService;
             _payUColombiaPaymentSettings = payUColombiaPaymentSettings;
             _settingService = settingService;
             _currencyService = currencyService;
             _currencySettings = currencySettings;
             _webHelper = webHelper;
            _httpContextAccessor = httpContextAccessor;
            _addressService = addressService;
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
            var myUtility = new PayUColombiaHelper();
            var orderId = postProcessPaymentRequest.Order.Id;
            var remotePostHelper = new Dictionary<string, string>();
            var roundedOrderTotal = Convert.ToInt32(postProcessPaymentRequest.Order.OrderTotal);
            var roundedOrderTotalExcTaxSub = Convert.ToInt32(postProcessPaymentRequest.Order.OrderSubtotalExclTax);
            var roundedOrderTotalTax = Convert.ToInt32(postProcessPaymentRequest.Order.OrderTax);
            remotePostHelper.Add("merchantId", _payUColombiaPaymentSettings.MerchantID);
            remotePostHelper.Add("accountId", _payUColombiaPaymentSettings.AccountID);
            remotePostHelper.Add("description", "Compra PayU - NopEcommerce");
            remotePostHelper.Add("referenceCode", orderId.ToString());
            remotePostHelper.Add("amount", roundedOrderTotal.ToString());
            remotePostHelper.Add("tax", roundedOrderTotalTax.ToString());
            remotePostHelper.Add("taxReturnBase", roundedOrderTotalExcTaxSub.ToString());
            remotePostHelper.Add("currency", _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode);
            remotePostHelper.Add("signature", myUtility.Getchecksum(_payUColombiaPaymentSettings.ApiKey.ToString(), _payUColombiaPaymentSettings.MerchantID.ToString(),
               postProcessPaymentRequest.Order.Id.ToString(), roundedOrderTotal.ToString(), _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode));
            remotePostHelper.Add("responseUrl", _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayUColombia/Return");
            remotePostHelper.Add("confirmationUrl", _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayUColombia/Return");
            remotePostHelper.Add("test", _payUColombiaPaymentSettings.UseSandbox ? "1" : "0");
            remotePostHelper.Add("lng", "es");

            //choosing correct order address
            var orderAddress = _addressService.GetAddressById(
                (postProcessPaymentRequest.Order.PickupInStore ? postProcessPaymentRequest.Order.PickupAddressId : postProcessPaymentRequest.Order.ShippingAddressId) ?? 0);


            remotePostHelper.Add("shippingAddress", orderAddress?.Address1);
                remotePostHelper.Add("shippingCity", orderAddress?.City);
                
                     
            if (postProcessPaymentRequest.Order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                remotePostHelper.Add("buyerFullName", orderAddress?.FirstName + " " + orderAddress?.LastName);
                remotePostHelper.Add("buyerEmail", orderAddress?.Email);
                remotePostHelper.Add("telephone", orderAddress?.PhoneNumber);
                
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
                FormName = "PayUColombiaForm",
                Url = _payUColombiaPaymentSettings.PayUColombiaUri,
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
            return _payUColombiaPaymentSettings.AdditionalFee;
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

            //PayUColombia is the redirection payment method
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
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentPayUColombia/Configure";
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
            return "PaymentPayUColombia";
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
            controllerName = "PaymentPayUColombia";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.PayUColombia.Controllers" }, { "area", null } };
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
            controllerName = "PaymentPayUColombia";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.PayUColombia.Controllers" }, { "area", null } };
        }


        */
        public Type GetControllerType()
        {
            return typeof(PaymentPayUColombiaController);
        }

        public override void Install()
        {
            var settings = new PayUColombiaPaymentSettings()
            {
                AccountID = "",
                ApiLogin = "",
                ApiKey = "",
                MerchantID = "",
                PayUColombiaUri = "https://test.payu.in/_payment",
                AdditionalFee = 0,
            };
            _settingService.SaveSetting(settings);

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.AccountID.Hint", "Ingrese su AccountID de su cuenta de PayU.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.AccountID", "AccountID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.RedirectionTip", "Será redirigido al sitio de PayU Colombia para completar el pedido.");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.ApiLogin", "ApiLogin");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.ApiLogin.Hint", "Ingrese su ApiLogin de su cuenta de PayU.");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.ApiKey", "ApiKey");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.ApiKey.Hint", "Ingrese su ApiKey de su cuenta de PayU.");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.MerchantID", "MerchantID");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.MerchantID.Hint", "Ingrese su MerchantID de su cuenta de PayU.");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.PayUColombiaUri", "Pay URI");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.PayUColombiaUri.Hint", "Ingrese la Url de su plataforma de pagos PayU");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.AdditionalFee", "Tarifa Adicional");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.AdditionalFee.Hint", "Ingrese una tarifa adicional para cobrar a sus clientes.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.UseSandbox", "Use Sandbox");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.UseSandbox.Hint", "Verificar habilitar Sandbox (Escenario de pruebas).");

            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.AccountID.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.AccountID");
            _localizationService. DeletePluginLocaleResource("Plugins.Payments.PayUColombia.RedirectionTip");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.ApiLogin");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.ApiLogin.Hint");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.ApiKey");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.ApiKey.Hint");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.MerchantID");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.MerchantID.Hint");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.PayUColombiaUri");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.PayUColombiaUri.Hint");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.AdditionalFee");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.AdditionalFee.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.UseSandbox");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.UseSandbox.Hint");

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
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayUColombia site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.PayUColombia.RedirectionTip"); }
        }

        /*
        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "PaymentPayUColombia";
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
