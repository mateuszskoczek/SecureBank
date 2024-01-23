using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Identity.Client;
using SecureBank.API.Authentication;
using SecureBank.API.Services;
using SecureBank.Authentication;
using SecureBank.Common;
using SecureBank.Common.Accounts;
using SecureBank.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SecureBank.API.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    public class AccountsController : ControllerBase
    {
        #region SERVICES

        private IAccountsService _accountsService;

        #endregion



        #region CONSTRUCTORS

        public AccountsController(IAccountsService accountsService)
        {
            _accountsService = accountsService;
        }

        #endregion



        #region METHODS

        [HttpPost]
        [Route("create-account")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [RequiresClaim("admin", "True")]
        public async Task<ActionResult<APIResponse<int>>> CreateAccount([FromBody] CreateAccountRequest data)
        {
            APIResponse<int> response = await _accountsService.CreateAccount(data);
            return response.Status switch
            {
                ResponseStatus.Ok => Ok(response),
                ResponseStatus.BadRequest => BadRequest(response),
                ResponseStatus.Unauthorized => Unauthorized(response),
            };
        }

        [HttpGet]
        [Route("{account_id}/password-variant")]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse<GetPasswordVariantResponse>>> GetPasswordVariant([FromRoute(Name = "account_id")] int accountId)
        {
            APIResponse<GetPasswordVariantResponse> response = await _accountsService.GetPasswordVariant(accountId);
            return response.Status switch
            {
                ResponseStatus.Ok => Ok(response),
                ResponseStatus.BadRequest => BadRequest(response),
                ResponseStatus.Unauthorized => Unauthorized(response),
            };
        }

        [HttpPost]
        [Route("authentication")]
        [AllowAnonymous]
        /*
         * Action codes:
         * 1 - Go back to client code input
         * 2 - Change password required
         */
        public async Task<ActionResult<APIResponse<string>>> Authentication([FromBody] AuthenticationRequest data)
        {
            APIResponse<string> response = await _accountsService.Authentication(data);
            return response.Status switch
            {
                ResponseStatus.Ok => Ok(response),
                ResponseStatus.BadRequest => BadRequest(response),
                ResponseStatus.Unauthorized => Unauthorized(response),
            };
        }

        [HttpPost]
        [Route("authentication-refresh")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<APIResponse<string>>> AuthenticationRefresh()
        {
            APIResponse<string> response = await _accountsService.AuthenticationRefresh(new Claims(User.Claims));
            return response.Status switch
            {
                ResponseStatus.Ok => Ok(response),
                ResponseStatus.BadRequest => BadRequest(response),
                ResponseStatus.Unauthorized => Unauthorized(response),
            };
        }

        [HttpPatch]
        [Route("change-password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<APIResponse>> ChangePassword([FromBody] ChangePasswordRequest data)
        {
            APIResponse response = await _accountsService.ChangePassword(new Claims(User.Claims), data);
            return response.Status switch
            {
                ResponseStatus.Ok => Ok(response),
                ResponseStatus.BadRequest => BadRequest(response),
                ResponseStatus.Unauthorized => Unauthorized(response),
            };
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<APIResponse<IEnumerable<AccountResponse>>>> GetAccounts([FromQuery]int? id, [FromQuery] string? iban)
        {
            APIResponse<IEnumerable<AccountResponse>> response = await _accountsService.GetAccounts(iban, id, new Claims(User.Claims));
            return response.Status switch
            {
                ResponseStatus.Ok => Ok(response),
                ResponseStatus.BadRequest => BadRequest(response),
                ResponseStatus.Unauthorized => Unauthorized(response),
            };
        }

        [HttpPatch]
        [Route("{account_id}/reset-password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [RequiresClaim("admin", "True")]
        public async Task<ActionResult<APIResponse>> ResetPassword([FromRoute(Name = "account_id")] int accountId)
        {
            APIResponse response = await _accountsService.ResetPassword(accountId);
            return response.Status switch
            {
                ResponseStatus.Ok => Ok(response),
                ResponseStatus.BadRequest => BadRequest(response),
                ResponseStatus.Unauthorized => Unauthorized(response),
            };
        }

        [HttpPatch]
        [Route("{account_id}/unlock")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [RequiresClaim("admin", "True")]
        public async Task<ActionResult<APIResponse>> UnlockAccount([FromRoute(Name = "account_id")] int accountId)
        {
            APIResponse response = await _accountsService.UnlockAccount(accountId);
            return response.Status switch
            {
                ResponseStatus.Ok => Ok(response),
                ResponseStatus.BadRequest => BadRequest(response),
                ResponseStatus.Unauthorized => Unauthorized(response),
            };
        }

        #endregion
    }
}
