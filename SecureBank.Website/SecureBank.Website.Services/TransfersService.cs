using SecureBank.Common.Accounts;
using SecureBank.Common;
using SecureBank.Website.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecureBank.Common.Transfers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SecureBank.Website.Services
{
    public interface ITransfersService
    {
        Task<APIResponse> CreateAdminTransfer(CreateAdminTransferRequest data);
        Task<APIResponse> CreateUserTransfer(CreateUserTransferRequest data);
        Task<APIResponse<IEnumerable<TransferResponse>>> GetTransfers();
        Task<APIResponse<IEnumerable<TransferResponse>>> GetUserTransfers(int accountId);
    }



    public class TransfersService : ITransfersService
    {
        #region FIELDS

        private readonly APIClient _apiClient;
        private readonly APIEndpointsConfiguration _configuration;

        #endregion



        #region CONSTRUCTORS

        public TransfersService(APIClient apiClient, APIEndpointsConfiguration configuration)
        {
            _apiClient = apiClient;
            _configuration = configuration;
        }

        #endregion



        #region METHODS

        public async Task<APIResponse<IEnumerable<TransferResponse>>> GetTransfers()
        {
            return await _apiClient.SendAsync<IEnumerable<TransferResponse>>(APIMethodType.GET, _configuration.TransfersGetTransfers);
        }

        public async Task<APIResponse<IEnumerable<TransferResponse>>> GetUserTransfers(int accountId)
        {
            string url = string.Format(_configuration.TransfersGetUserTransfers, accountId);
            return await _apiClient.SendAsync<IEnumerable<TransferResponse>>(APIMethodType.GET, url);
        }

        public async Task<APIResponse> CreateAdminTransfer(CreateAdminTransferRequest data)
        {
            return await _apiClient.SendAsync(APIMethodType.POST, _configuration.TransfersCreateAdminTransfer, data);
        }

        public async Task<APIResponse> CreateUserTransfer(CreateUserTransferRequest data)
        {
            return await _apiClient.SendAsync(APIMethodType.POST, _configuration.TransfersCreateUserTransfer, data);
        }

        #endregion
    }
}
