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

using Nop.Plugin.Payments.PayZonaPagos.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Web.Framework;

using System.Threading.Tasks;
using LoginApi;
using Newtonsoft.Json.Linq;
using Nop.Services.Common;

namespace Nop.Plugin.Payments.PayZonaPagos
{
    /// <summary>
    /// PayZonaPagos payment processor
    /// </summary>
    public class PayZonaPagosPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly PayZonaPagosPaymentSettings _payUColombiaPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IAddressService _addressService;

        #endregion

        #region Ctor

        public PayZonaPagosPaymentProcessor(PayZonaPagosPaymentSettings payUColombiaPaymentSettings,
            IAddressService addressService,
            ISettingService settingService, 
              ILocalizationService localizationService,
              IHttpContextAccessor httpContextAccessor,
              IWebHelper webHelper)
        {
             _localizationService = localizationService;
             _payUColombiaPaymentSettings = payUColombiaPaymentSettings;
             _settingService = settingService;
             _webHelper = webHelper;
            _httpContextAccessor = httpContextAccessor;
            _addressService = addressService;
        }

        #endregion

        #region Utilities

        public async Task<Tuple<int, string>> AgregarPago(JObject model)
        {
            try
            {              

                // Petición Post
                var apiUtil = await ApiUtilFactory.GetApiUtil(_payUColombiaPaymentSettings);

                // Envia peticion Get
                var response = await apiUtil.SendRequestPostAsync(model);

                if (response.Item1 == 200 /*ok*/)
                {
                    return response;
                }
                else
                {
                    var ex = new Exception($"{response.Item1.ToString()}, {response.Item2.ToString()}");
                    //new LogUtil().GuardarLog(modulo: "ApiLogin > LoginApi_Register> ", exception: ex);

                    return response;
                };

            }
            catch (Exception ex)
            {
                //new LogUtil().GuardarLog(modulo: "ApiLogin > LoginApi_Register> ", exception: ex);
                return new Tuple<int, string>(404, "Error sistema");
            }
        }

        #endregion

        #region Methods


        /// <summary>
        /// Create common query parameters for the request
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Created post parameters</returns>
        private JObject CreatePostParameters(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var orderId = postProcessPaymentRequest.Order.Id;
            var remotePostHelper = new Dictionary<string, string>();
            var roundedOrderTotal = Convert.ToInt32(postProcessPaymentRequest.Order.OrderTotal);
            var roundedOrderTotalIncTaxSub = Convert.ToInt32(postProcessPaymentRequest.Order.OrderSubtotalInclTax);
            var roundedOrderTotalTax = Convert.ToInt32(postProcessPaymentRequest.Order.OrderTax);
            var roundedOrderTotalIncShipping = 0;
            var roundedOrderTotalIncFee = Convert.ToInt32(postProcessPaymentRequest.Order.PaymentMethodAdditionalFeeInclTax);
            var responseUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentPayZonaPagos/Return";
            var nombreCliente = string.Empty;
            var apellidoCliente = string.Empty;
            var emailCliente = string.Empty;
            var telefonoCliente = string.Empty;

            //choosing correct order address
            var orderAddress = _addressService.GetAddressById(
                (postProcessPaymentRequest.Order.PickupInStore ? postProcessPaymentRequest.Order.PickupAddressId : postProcessPaymentRequest.Order.ShippingAddressId) ?? 0);


            if (postProcessPaymentRequest.Order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                nombreCliente = orderAddress?.FirstName;
                apellidoCliente = orderAddress?.LastName;
                emailCliente = orderAddress?.Email;
                telefonoCliente = orderAddress?.PhoneNumber;
                roundedOrderTotalIncShipping = Convert.ToInt32(postProcessPaymentRequest.Order.OrderShippingInclTax);
            }

            var jsonObject = new JObject
{

    { "total_con_iva", roundedOrderTotalIncTaxSub },
    { "valor_iva", roundedOrderTotalTax },
    { "comision", roundedOrderTotalIncFee },
    {"descripcion_pago", "Compra ZonaPago - NopEcommerce" },
    {"email", emailCliente},
    {"Id_cliente", "123456"},
    {"tipo_id", "C"},
    {"Origen", "ecommerce"},
    {"Pasarela", "zona_virtual"},
    {"nombre_cliente", nombreCliente},
    {"apellido_client", apellidoCliente},
    {"telefono_cliente", telefonoCliente},
    {"info_opcional1", responseUrl},
    {"info_opcional2", "0"},
    {"info_opcional3", roundedOrderTotalIncShipping},//Shipping
    { "Pagos", new JArray( new JObject
        {
            {"Id", 0},
            {"Id_Pago", 0},
            {"NumeroCuenta", orderId },
            { "Valor", roundedOrderTotal },
            { "Valor1", 0},
            {"Valor2", 0},
            {"Valor3", 0},
            {"Valor4", ""},
            {"Valor5", ""},
            {"Valor6", ""},
            {"Valor7", ""},
            {"Comision", 0},
            {"Descripcion", ""},
            {"Descripcion1", ""},
            {"Descripcion2", ""},
            {"Descripcion3", ""},
            {"Descripcion4", ""},
            {"Descripcion5", ""},
            {"Descripcion6", ""},
            {"Descripcion7", ""},
            {"Tipo", 0},
            {"IdExterno", ""}
        }
      )
    }
};

            return jsonObject;

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
            var remotePostJsonData = CreatePostParameters(postProcessPaymentRequest);
            var stringJson = remotePostJsonData.ToString();
            var response = AgregarPago(remotePostJsonData);
            if(response.Result.Item1 == 200)
            {
                var userObj = JObject.Parse(response.Result.Item2);
                _httpContextAccessor.HttpContext.Response.Redirect(Convert.ToString(userObj["_Ruta"]));                
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

            //PayZonaPagos is the redirection payment method
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
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentPayZonaPagos/Configure";
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
            return "PaymentPayZonaPagos";
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
            controllerName = "PaymentPayZonaPagos";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.PayZonaPagos.Controllers" }, { "area", null } };
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
            controllerName = "PaymentPayZonaPagos";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.PayZonaPagos.Controllers" }, { "area", null } };
        }


        */
        public Type GetControllerType()
        {
            return typeof(PaymentPayZonaPagosController);
        }

        public override void Install()
        {
            var settings = new PayZonaPagosPaymentSettings()
            {
                Uri = "http://pagos.estrategiasegura.biz",
                RutaMetodo = "/api/pagos/agregar"

            };
            _settingService.SaveSetting(settings);

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.Uri.Hint", "Ingrese Ingrese la Url de la Api de ZonaPagos.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.Uri", "Uri");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.RedirectionTip", "Será redirigido al sitio de ZonaPagos para completar el pedido.");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.Nombre", "Nombre");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.Nombre.Hint", "Ingrese el Nombre de su cuenta de ZonaPagos.");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.TipoSeguridad", "Tipo de Seguridad");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.TipoSeguridad.Hint", "Ingrese el tipo de seguridad de su Api.");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.DirectorioVirtual", "Directorio Virtual");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.DirectorioVirtual.Hint", "Ingrese su Directorio Virtual de su cuenta de ZonaPagos.");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.Token", "Token");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.Token.Hint", "Ingrese el Token de su Api");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.Usuario", "Usuario Api");
             _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.Usuario.Hint", "Ingrese su usuario de la Api.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.Clave", "Clave");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.Clave.Hint", "Ingrese su Clave de la Api.");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.RutaToken", "Ruta de Token");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.RutaToken.Hint", "Ingrese su Ruta token Api.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.EncabezadoToken", "Encabezado token Api");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.EncabezadoToken.Hint", "Ingrese su Encabezado token Api.");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.EncabezadoUsuario", "Encabezado usuario Api");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.EncabezadoUsuario.Hint", "Ingrese su Encabezado usuario Api.");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.EncabezadoClave", "Encabezado clave Api");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.EncabezadoClave.Hint", "Ingrese su Encabezado clave Api.");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.Prefijo", "Prefijo url Api");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.Prefijo.Hint", "Ingrese su Prefijo url Api.");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.RutaMetodo", "Ruta de Metodo");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayZonaPagos.RutaMetodo.Hint", "Ingrese su Ruta Metodo del Pago.");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.AdditionalFee", "Tarifa Adicional");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.PayUColombia.AdditionalFee.Hint", "Ingrese una tarifa adicional para cobrar a sus clientes.");

            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.Uri.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.Uri");
            _localizationService. DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.RedirectionTip");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.Nombre");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.Nombre.Hint");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.TipoSeguridad");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.TipoSeguridad.Hint");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.DirectorioVirtual");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.DirectorioVirtual.Hint");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.Token");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.Token.Hint");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.Usuario");
             _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.Usuario.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.Clave");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.Clave.Hint");

            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.RutaToken");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.RutaToken.Hint");

            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.EncabezadoToken");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.EncabezadoToken.Hint");

            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.EncabezadoUsuario");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.EncabezadoUsuario.Hint");

            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.EncabezadoClave");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.EncabezadoClave.Hint");

            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.Prefijo");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.Prefijo.Hint");

            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.RutaMetodo");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayZonaPagos.RutaMetodo.Hint");

            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.AdditionalFee");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.PayUColombia.AdditionalFee.Hint");


            base.Uninstall();
        }

        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _payUColombiaPaymentSettings.AdditionalFee;
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
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayZonaPagos site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.PayZonaPagos.RedirectionTip"); }
        }

        /*
        public void GetPublicViewComponent(out string viewComponentName)
        {
            viewComponentName = "PaymentPayZonaPagos";
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
