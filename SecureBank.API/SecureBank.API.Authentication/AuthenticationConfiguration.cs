using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.API.Authentication
{
    public class AuthenticationConfiguration
    {
        #region PROPERTIES

        // Token
        public string TokenKey { get; private set; }
        public string TokenIssuer { get; private set; }
        public string TokenAudience { get; private set; }
        public int TokenLifetime { get; private set; }

        #endregion



        #region CONSTRUCTORS

        public AuthenticationConfiguration(IConfiguration configuration)
        {
            TokenKey = configuration.GetSection("Authentication").GetSection("Token")["Key"];
            TokenIssuer = configuration.GetSection("Authentication").GetSection("Token")["Issuer"];
            TokenAudience = configuration.GetSection("Authentication").GetSection("Token")["Audience"];
            TokenLifetime = int.Parse(configuration.GetSection("Authentication").GetSection("Token")["Lifetime"]);
        }

        #endregion
    }
}
