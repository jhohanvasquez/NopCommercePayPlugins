using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LoginApi
{
    #region Administrador Api
    
   
    /// <summary>
    /// Tipos de seguridad del servicio.
    /// </summary>
    public enum TipoSeguridad
    {
        Simple,
        Token,
        UserPass,
        UserPassToken,
        All
    }

   
    public partial class ApiConfiguraciones_RutasDTO
    {
        public long Id { get; set; }
        public long IdApi { get; set; }
        public string Ruta { get; set; }
        public string Descripcion { get; set; }
        public string Identificador { get; set; }
    }

    /// <summary>
    /// Clase Token
    /// </summary>
    public class TokenDTO
    {
        public string Access_token { get; set; }
        public string Token_type { get; set; }
        public string Expires_in { get; set; }
    }

    #endregion


    #region Api Login


    public class PagosApi_ErroresDTO
    {
        public string Error { get; set; }
        public string Error_description { get; set; }
    }

    #endregion
}
