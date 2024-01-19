using Microsoft.AspNetCore.Components.Authorization;
using SecureBank.Common;
using SecureBank.Website.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.Website.Authentication
{
    public class TokenAuthenticationStateProvider : AuthenticationStateProvider
    {
        #region SERVICES

        private readonly IAccountsService _accountsService;

        private readonly AuthenticationHelper _authenticationHelper;

        private readonly HttpClient _httpClient;

        #endregion



        #region CONSTRUCTORS

        public TokenAuthenticationStateProvider(IAccountsService accountsService, AuthenticationHelper authenticationHelper, HttpClient httpClient) 
        { 
            _accountsService = accountsService;
            _authenticationHelper = authenticationHelper;
            _httpClient = httpClient;
        }

        #endregion



        #region PUBLIC METHODS

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            AuthenticationState state = new AuthenticationState(new ClaimsPrincipal());

            string token = await _authenticationHelper.GetToken();

            if (string.IsNullOrWhiteSpace(token))
            {
                return state;
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            APIResponse<string> refreshResponse = await _accountsService.AuthenticationRefresh();

            if (!refreshResponse.Success)
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return state;
            }

            token = refreshResponse.Data;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            state = new AuthenticationState(new ClaimsPrincipal()); //TODO: Add claims

            return state;
        }

        #endregion
    }
}
