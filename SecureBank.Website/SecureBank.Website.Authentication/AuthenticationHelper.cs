using Blazored.SessionStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.Website.Authentication
{
    public class AuthenticationHelper
    {
        #region CONSTANTS

        private const string TOKEN_KEY = "token";

        #endregion



        #region SERVICES

        private readonly ISessionStorageService _sessionStorageService;

        #endregion



        #region CONSTRUCTIONS

        public AuthenticationHelper(ISessionStorageService sessionStorageService)
        {
            _sessionStorageService = sessionStorageService;
        }

        #endregion



        #region PUBLIC METHODS

        public async Task<string> GetToken() => await _sessionStorageService.GetItemAsync<string>(TOKEN_KEY);

        public async Task SaveToken(string token) => await _sessionStorageService.SetItemAsync(TOKEN_KEY, token);

        public async Task RemoveToken() => await _sessionStorageService.RemoveItemAsync(TOKEN_KEY);

        #endregion
    }
}
