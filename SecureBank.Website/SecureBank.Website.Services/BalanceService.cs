using SecureBank.Common.Accounts;
using SecureBank.Common;
using SecureBank.Website.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.Website.Services
{
    public interface IBalanceService
    {
        Task<APIResponse<decimal>> GetAccountBalance(int accountId);
        Task<APIResponse<decimal>> GetBalance();
    }



    public class BalanceService : IBalanceService
    {
        #region FIELDS

        private readonly APIClient _apiClient;
        private readonly APIEndpointsConfiguration _configuration;

        #endregion



        #region CONSTRUCTORS

        public BalanceService(APIClient apiClient, APIEndpointsConfiguration configuration)
        {
            _apiClient = apiClient;
            _configuration = configuration;
        }

        #endregion



        #region METHODS

        public async Task<APIResponse<decimal>> GetAccountBalance(int accountId)
        {
            string url = string.Format(_configuration.BalanceGetAccountBalance, accountId);
            return await _apiClient.SendAsync<decimal>(APIMethodType.GET, url);
        }

        public async Task<APIResponse<decimal>> GetBalance()
        {
            return await _apiClient.SendAsync<decimal>(APIMethodType.GET, _configuration.BalanceGetBalance);
        }

        #endregion
    }
}
