using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Nop.Core;

namespace Nop.Plugin.Payments.PayWompi.Services
{
    /// <summary>
    /// Represents the HTTP client to request Wompi services
    /// </summary>
    public partial class WompiHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly PayWompiPaymentSettings _payWompiPaymentSettings;
        private Dictionary<string, string> Headers { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Agrega un Header en el encabezado de la petición.
        /// </summary>
        /// <param name="key">Nombre del Header.</param>
        /// <param name="value">Valor del Header.</param>
        public void LoadHeaders(string key, string value)
        {
            Headers.DefaultIfEmpty(); // Se limpia la lista de Headers.    
            Headers.Add(key, value);
        }

        /// <summary>
        /// Agrega una lista de Headers en el encabezado de la petición.
        /// </summary>
        /// <param name="headers">Lista de headers con los Key y Value.</param>
        public void LoadHeaders(Dictionary<string, string> headers)
        {
            Headers.DefaultIfEmpty(); // Se limpia la lista de Headers.            
            Headers = headers; // Se agrega la lista de headers.
        }

        public WompiHttpClient(HttpClient client,
            PayWompiPaymentSettings payWompiPaymentSettings)
        {
            //configure client
            client.Timeout = TimeSpan.FromMilliseconds(5000);
            client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, $"nopCommerce-{NopVersion.CurrentVersion}");
            _httpClient = client;
            _payWompiPaymentSettings = payWompiPaymentSettings;
            Headers = new Dictionary<string, string>();
            Headers.DefaultIfEmpty(); // Se limpia la lista de Headers. 
        }

        #endregion

        #region Methods

        public async Task<Tuple<int, string>> GetTransactionAsync(string transactionID)
        {
            //get response
            var _uri = "https://production.wompi.co/v1/transactions/" + transactionID;

            // Variables recibir respuesta
            string content = string.Empty;

            // Configuraciones de la petición
            _httpClient.BaseAddress = new Uri(_uri);

            // Se agregar los Headers a la petición
            foreach (KeyValuePair<string, string> header in Headers)
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);

            // Se realiza la petición al servicio
            HttpResponseMessage response = await _httpClient.GetAsync(_uri);

            // Se obtiene el contenido de la respuesta
            if (response.IsSuccessStatusCode)
                content = await response.Content.ReadAsStringAsync();

            return new Tuple<int, string>((int)response.StatusCode, content);
        }



        #endregion
    }
}