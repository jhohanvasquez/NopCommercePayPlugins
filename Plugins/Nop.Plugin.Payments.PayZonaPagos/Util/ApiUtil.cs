using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LoginApi
{
    /// <summary>
    /// Métodos predeterminados para consumir servicios Web Api
    /// </summary>
    public class ApiUtil
    {
        private readonly string _uri;
        private readonly string _virtualDirectory;
        private Dictionary<string, string> Headers { get; set; }
        public string _url;
        public string _usuario;
        public string _clave;
        public string _token;

        /// <summary>
        /// Constructor para inicializar el URI y DirectorioVirtual
        /// </summary>        
        /// </remarks><param name="uri">Dominio del servicio.</param>
        /// <param name="virtualDirectory">Si el api está en un directorio virtual del dominio.</param>      
        public ApiUtil(string uri, string virtualDirectory)
        {
            _uri = uri;
            _virtualDirectory = virtualDirectory;
            Headers = new Dictionary<string, string>();
            Headers.DefaultIfEmpty(); // Se limpia la lista de Headers. 
        }

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

        /// <summary>
        /// Envia la petición GET a la url, y con la lista de Headers.
        /// </summary>    
        /// <param name="url">Dirección a la que se realiza la petición.</param>
        /// <returns></returns>
        public async Task<Tuple<int, string>> SendRequestGetAsync()
        {
            // Variables recibir respuesta
            string content = string.Empty;
            HttpResponseMessage response = new HttpResponseMessage();

            // Se abre el cliente para realizar peticiones al API
            using (HttpClient client = new HttpClient())
            {
                // Configuraciones de la petición
                client.BaseAddress = new Uri(_uri);
                _url = string.Concat(_virtualDirectory, _url);

                // Se agregar los Headers a la petición
                foreach (KeyValuePair<string, string> header in Headers) client.DefaultRequestHeaders.Add(header.Key, header.Value);
                
                // Se realiza la petición al servicio
                response = await client.GetAsync(_url);
            }

            // Se obtiene el contenido de la respuesta
            if (response.IsSuccessStatusCode) content = await response.Content.ReadAsStringAsync();

            return new Tuple<int, string>((int)response.StatusCode, content);
        }

        /// <summary>
        /// Envía un objeto T en el cuerpo de la peticion POST.
        /// </summary>
        /// <typeparam name="T">T es un clase genérica.</typeparam>
        /// <param name="dataModel">Instancia con los datos, serán serializados como un objeto JSON.</param>
        /// <param name="url">Url a la que se realiza la petición.</param>
        /// <returns>Retorna un código StatusCode estandar Http y un String con el contenidos de la respuesta.</returns>
        public async Task<Tuple<int, string>> SendRequestPostAsync<T>(T dataModel) where T : class
        {
            // Respuesta de la petición
            string result = string.Empty;

            // Datos para realizar la petición al servicio.
            _url = string.Concat(_virtualDirectory, _url);
            string data = JsonConvert.SerializeObject(dataModel);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            // Se hace la petición y se guarda la respuesta.
            HttpResponseMessage response = new HttpResponseMessage();
            using (HttpClient client = new HttpClient())
            {
                // Se agrega la dirección del dominio al cliente.
                client.BaseAddress = new Uri(_uri);

                // Se agregar los Headers a la petición
                foreach (KeyValuePair<string, string> header in Headers) client.DefaultRequestHeaders.Add(header.Key, header.Value);

                // Se hace la petición post, y se guarda la respuesta.
                response = await client.PostAsync(_url, content);
            }

            // Si la respuesta fue correcta, se obtiene su contenido como un string. 
            result = await response.Content.ReadAsStringAsync();
            return new Tuple<int, string>((int)response.StatusCode, result);
        }

        /// <summary>
        /// Envía una collección de pares clave/valor en el cuerpo de la peticion POST.
        /// </summary>
        /// <param name="encodedContent">Contenido encode que se envía usando el tipo application/x-www-form-urlencoded MIME.</param>
        /// <param name="url">Url a la que se realiza la petición.</param>
        /// <returns>Retorna un código StatusCode estandar Http y un String con el contenidos de la respuesta.</returns>
        public async Task<Tuple<int, string>> SendRequestPostAsync(List<KeyValuePair<string, string>> encodedContent, string url)
        {
            //Se indica el tipo de contenido (Content-Type). El contenido se envía como parte de la url.
            HttpContent contentUrl = new FormUrlEncodedContent(encodedContent);

            //Se construye la url        
            url = string.Concat(_virtualDirectory, url);

            // Se realiza la petición al servicio, y se cierra la conexión después de obtener la respuesta.
            HttpResponseMessage response = new HttpResponseMessage();
            using (HttpClient client = new HttpClient())
            {
                //Se hace el request al web api
                client.BaseAddress = new Uri(_uri);
                response = client.PostAsync(url, contentUrl).Result;
            }

            // Se obtiene el contenido de la respuesta
            string content = string.Empty;
            if (response.IsSuccessStatusCode || (int)response.StatusCode == 400) content = await response.Content.ReadAsStringAsync();

            return new Tuple<int, string>((int)response.StatusCode, content);
        }

        /// <summary>
        /// Envía un objeto T en el cuerpo de la peticion PUT.
        /// </summary>
        /// <typeparam name="T">T es un clase genérica.</typeparam>
        /// <param name="dataModel">Instancia con los datos, serán serializados como un objeto JSON.</param>
        /// <param name="url">Url a la que se realiza la petición.</param>
        /// <returns>Retorna un código StatusCode estandar Http y un String con el contenidos de la respuesta.</returns>
        public async Task<Tuple<int, string>> SendRequestPutAsync<T>(T dataModel) where T : class
        {
            // Respuesta de la petición
            string result = string.Empty;
            // Datos para realizar la petición al servicio.
            _url = string.Concat(_virtualDirectory, _url);
            string data = JsonConvert.SerializeObject(dataModel);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            // Se hace la petición y se guarda la respuesta.
            HttpResponseMessage response = new HttpResponseMessage();
            using (HttpClient client = new HttpClient())
            {
                // Se agrega la dirección del dominio al cliente.
                client.BaseAddress = new Uri(_uri);

                // Se agregar los Headers a la petición
                foreach (KeyValuePair<string, string> header in Headers) client.DefaultRequestHeaders.Add(header.Key, header.Value);

                // Se hace la petición post, y se guarda la respuesta.
                response = await client.PutAsync(_url, content);
            }

            // Si la respuesta fue correcta, se obtiene su contenido como un string. 
            result = await response.Content.ReadAsStringAsync();
            return new Tuple<int, string>((int)response.StatusCode, result);
        }

        /// <summary>
        /// Envia la petición DELETE a la url, y con la lista de Headers.
        /// </summary>    
        /// <param name="url">Dirección a la que se realiza la petición.</param>
        /// <returns></returns>
        public async Task<Tuple<int, string>> SendRequestDeleteAsync()
        {
            // Variables recibir respuesta
            string content = string.Empty;
            HttpResponseMessage response = new HttpResponseMessage();

            // Se abre el cliente para realizar peticiones al API
            using (HttpClient client = new HttpClient())
            {
                // Configuraciones de la petición
                client.BaseAddress = new Uri(_uri);
                _url = string.Concat(_virtualDirectory, _url);

                // Se agregar los Headers a la petición
                foreach (KeyValuePair<string, string> header in Headers) client.DefaultRequestHeaders.Add(header.Key, header.Value);

                // Se realiza la petición al servicio
                response = await client.DeleteAsync(_url);
            }

            // Se obtiene el contenido de la respuesta
            if (response.IsSuccessStatusCode) content = await response.Content.ReadAsStringAsync();

            return new Tuple<int, string>((int)response.StatusCode, content);
        }
    }
}