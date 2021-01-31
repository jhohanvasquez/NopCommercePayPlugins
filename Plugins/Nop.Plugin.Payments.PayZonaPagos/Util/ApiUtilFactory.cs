using AutoMapper;
using Newtonsoft.Json;
using Nop.Plugin.Payments.PayZonaPagos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoginApi
{
    /// <summary>
    /// Gestión de las instancias de ApiUtil dependiendo del tipo de seguridad.
    /// </summary>
    /// <remarks>Se usa el patrón de diseño Simple Factory.</remarks>
    public static class ApiUtilFactory
    {
        /// <summary>
        /// Obtiene la instancia de ApiUtil.
        /// </summary>
        /// <param name="api">Credenciales de usuario necesarios según el tipo de seguridad.</param>
        /// <returns>Instancia de ApiUtil</returns>
        public static async Task<ApiUtil> GetApiUtil(PayZonaPagosPaymentSettings api)
        {
            
            ApiUtil apiUtil = new ApiUtil(api.Uri ?? "", api.DirectorioVirtual ?? "")
            {
                _url = api.RutaMetodo,
                _usuario = api.Usuario,
                _clave = api.Clave,
                _token = api.Token
            };

            switch (api.TipoSeguridad)
            {
                case TipoSeguridad.Simple:
                    break;
                case TipoSeguridad.Token:
                    apiUtil.LoadHeaders($"{api.EncabezadoToken}", $"{api.Prefijo?.Trim()}  {api.Token}");
                    break;
                case TipoSeguridad.UserPass:
                    apiUtil.LoadHeaders(new Dictionary<string, string>
                    {
                        { $"{api.EncabezadoUsuario}", api.Usuario},
                        { $"{api.EncabezadoClave}", api.Clave}
                    });
                    break;
                case TipoSeguridad.UserPassToken:
                    // Credenciales para solicitar el token.
                    List<KeyValuePair<string, string>> credenciales = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>($"{api.EncabezadoUsuario}", api.Usuario),
                        new KeyValuePair<string, string>($"{api.EncabezadoClave}", api.Clave)
                    };
                    Tuple<int, string> respuestaToken = await apiUtil.SendRequestPostAsync(credenciales, api.RutaToken);
                    TokenDTO token = new TokenDTO();
                    if (respuestaToken.Item1 == 200)
                    {
                        token = JsonConvert.DeserializeObject<TokenDTO>(respuestaToken.Item2);
                        apiUtil.LoadHeaders($"{api.EncabezadoToken}", $"{api.Prefijo?.Trim()}  {token.Access_token}");
                    }
                    break;
                case TipoSeguridad.All:
                    break;
                default:
                    break;
            }
            return apiUtil;
        }

        

    }
}