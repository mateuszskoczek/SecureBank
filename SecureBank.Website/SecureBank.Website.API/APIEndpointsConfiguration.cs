using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.Website.API
{
    public class APIEndpointsConfiguration
    {
        #region PROPERTIES

        public string Base { get; private set; }

        // Accounts
        public string AccountsBase { get; private set; }
        public string AccountsCreateAccount { get; private set; }
        public string AccountsGetPasswordVariant { get; private set; }
        public string AccountsAuthentication { get; private set; }
        public string AccountsAuthenticationRefresh { get; private set; }

        #endregion



        #region CONSTRUCTORS

        public APIEndpointsConfiguration(IConfiguration configuration)
        {
            Base = configuration.GetSection("Endpoints")["Base"];

            AccountsBase = $"{Base}{configuration.GetSection("Endpoints").GetSection("Accounts")["Base"]}";
            AccountsCreateAccount = $"{AccountsBase}{configuration.GetSection("Endpoints").GetSection("Accounts")["CreateAccount"]}";
            AccountsGetPasswordVariant = $"{AccountsBase}{configuration.GetSection("Endpoints").GetSection("Accounts")["GetPasswordVariant"]}";
            AccountsAuthentication = $"{AccountsBase}{configuration.GetSection("Endpoints").GetSection("Accounts")["Authentication"]}";
            AccountsAuthenticationRefresh = $"{AccountsBase}{configuration.GetSection("Endpoints").GetSection("Accounts")["AuthenticationRefresh"]}";
        }

        #endregion
    }
}
