using SecureBank.Common;
using SecureBank.Common.Accounts;
using SecureBank.Website.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.Website.Services
{
    public interface IAccountsService
    {
        Task<APIResponse<int>> CreateAccount(CreateAccountRequest data);
        Task<APIResponse<GetPasswordVariantResponse>> GetPasswordVariant(int accountId);
        Task<APIResponse<string>> Authentication(int accountId, AuthenticationRequest data);
        Task<APIResponse<string>> AuthenticationRefresh();
        Task<APIResponse> ChangePassword(ChangePasswordRequest data);
        Task<APIResponse<IEnumerable<AccountResponse>>> GetAccounts(int? id = null, string? iban = null);
        Task<APIResponse> ResetPassword(int accountId);
        Task<APIResponse> UnlockAccount(int accountId);
    }



    public class AccountsService : IAccountsService
    {
        #region FIELDS

        private readonly APIClient _apiClient;
        private readonly APIEndpointsConfiguration _configuration;

        #endregion



        #region CONSTRUCTORS

        public AccountsService(APIClient apiClient, APIEndpointsConfiguration configuration)
        {
            _apiClient = apiClient;
            _configuration = configuration;
        }

        #endregion



        #region METHODS

        public async Task<APIResponse<int>> CreateAccount(CreateAccountRequest data)
        {
            return await _apiClient.SendAsync<int, CreateAccountRequest>(APIMethodType.POST, _configuration.AccountsCreateAccount, data);
        }

        public async Task<APIResponse<GetPasswordVariantResponse>> GetPasswordVariant(int accountId)
        {
            string url = string.Format(_configuration.AccountsGetPasswordVariant, accountId);
            return await _apiClient.SendAsync<GetPasswordVariantResponse>(APIMethodType.GET, url);
        }

        public async Task<APIResponse<string>> Authentication(int accountId, AuthenticationRequest data)
        {
            return await _apiClient.SendAsync<string, AuthenticationRequest>(APIMethodType.POST, _configuration.AccountsAuthentication, data);
        }

        public async Task<APIResponse<string>> AuthenticationRefresh()
        {
            return await _apiClient.SendAsync<string>(APIMethodType.POST, _configuration.AccountsAuthenticationRefresh);
        }

        public async Task<APIResponse> ChangePassword(ChangePasswordRequest data)
        {
            return await _apiClient.SendAsync(APIMethodType.PATCH, _configuration.AccountsChangePassword, data);
        }

        public async Task<APIResponse<IEnumerable<AccountResponse>>> GetAccounts(int? id = null, string? iban = null)
        {
            Dictionary<string, string> query = new Dictionary<string, string>();
            if (id.HasValue)
            {
                query.Add("id", id.Value.ToString());
            }
            if (iban is not null)
            {
                query.Add("iban", iban);
            }
            return await _apiClient.SendAsync<IEnumerable<AccountResponse>>(APIMethodType.GET, _configuration.AccountsGetAccounts, query);
        }

        public async Task<APIResponse> ResetPassword(int accountId)
        {
            string url = string.Format(_configuration.AccountsResetPassword, accountId);
            return await _apiClient.SendAsync(APIMethodType.PATCH, url);
        }

        public async Task<APIResponse> UnlockAccount(int accountId)
        {
            string url = string.Format(_configuration.AccountsUnlockAccount, accountId);
            return await _apiClient.SendAsync(APIMethodType.PATCH, url);
        }

        #endregion
    }
}
