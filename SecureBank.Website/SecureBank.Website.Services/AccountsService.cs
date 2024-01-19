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
            string url = string.Format(_configuration.AccountsAuthentication, accountId);
            return await _apiClient.SendAsync<string, AuthenticationRequest>(APIMethodType.POST, url, data);
        }

        public async Task<APIResponse<string>> AuthenticationRefresh()
        {
            return await _apiClient.SendAsync<string>(APIMethodType.POST, _configuration.AccountsAuthenticationRefresh);
        }

        #endregion
    }
}
