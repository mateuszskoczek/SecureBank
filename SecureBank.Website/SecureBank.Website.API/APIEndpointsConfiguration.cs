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
        public string AccountsChangePassword { get; private set; }
        public string AccountsGetPasswordVariant { get; private set; }
        public string AccountsAuthentication { get; private set; }
        public string AccountsAuthenticationRefresh { get; private set; }
        public string AccountsGetAccounts { get; private set; }
        public string AccountsResetPassword { get; private set; }
        public string AccountsUnlockAccount { get; private set; }

        // Balance
        public string BalanceBase { get; private set; }
        public string BalanceGetAccountBalance { get; private set; }
        public string BalanceGetBalance { get; private set; }

        // Transfers
        public string TransfersBase { get; private set; }
        public string TransfersGetUserTransfers { get; private set; }
        public string TransfersGetTransfers { get; private set; }
        public string TransfersCreateAdminTransfer { get; private set; }
        public string TransfersCreateUserTransfer { get; private set; }

        #endregion



        #region CONSTRUCTORS

        public APIEndpointsConfiguration(IConfiguration configuration)
        {
            Base = configuration.GetSection("Endpoints")["Base"];

            AccountsBase = $"{Base}{configuration.GetSection("Endpoints").GetSection("Accounts")["Base"]}";
            AccountsCreateAccount = $"{AccountsBase}{configuration.GetSection("Endpoints").GetSection("Accounts")["CreateAccount"]}";
            AccountsChangePassword = $"{AccountsBase}{configuration.GetSection("Endpoints").GetSection("Accounts")["ChangePassword"]}";
            AccountsGetPasswordVariant = $"{AccountsBase}{configuration.GetSection("Endpoints").GetSection("Accounts")["GetPasswordVariant"]}";
            AccountsAuthentication = $"{AccountsBase}{configuration.GetSection("Endpoints").GetSection("Accounts")["Authentication"]}";
            AccountsAuthenticationRefresh = $"{AccountsBase}{configuration.GetSection("Endpoints").GetSection("Accounts")["AuthenticationRefresh"]}";
            AccountsGetAccounts = $"{AccountsBase}{configuration.GetSection("Endpoints").GetSection("Accounts")["GetAccounts"]}";
            AccountsResetPassword = $"{AccountsBase}{configuration.GetSection("Endpoints").GetSection("Accounts")["ResetPassword"]}";
            AccountsUnlockAccount = $"{AccountsBase}{configuration.GetSection("Endpoints").GetSection("Accounts")["UnlockAccount"]}";

            BalanceBase = $"{Base}{configuration.GetSection("Endpoints").GetSection("Balance")["Base"]}";
            BalanceGetAccountBalance = $"{BalanceBase}{configuration.GetSection("Endpoints").GetSection("Balance")["GetAccountBalance"]}";
            BalanceGetBalance = $"{BalanceBase}{configuration.GetSection("Endpoints").GetSection("Balance")["GetBalance"]}";

            TransfersBase = $"{Base}{configuration.GetSection("Endpoints").GetSection("Transfers")["Base"]}";
            TransfersGetTransfers = $"{TransfersBase}{configuration.GetSection("Endpoints").GetSection("Transfers")["GetTransfers"]}";
            TransfersGetUserTransfers = $"{TransfersBase}{configuration.GetSection("Endpoints").GetSection("Transfers")["GetUserTransfers"]}";
            TransfersCreateAdminTransfer = $"{TransfersBase}{configuration.GetSection("Endpoints").GetSection("Transfers")["CreateAdminTransfer"]}";
            TransfersCreateUserTransfer = $"{TransfersBase}{configuration.GetSection("Endpoints").GetSection("Transfers")["CreateUserTransfer"]}";
        }

        #endregion
    }
}
