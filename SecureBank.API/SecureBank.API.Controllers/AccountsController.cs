using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using SecureBank.API.Services;
using SecureBank.Common;
using SecureBank.Common.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
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
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse<int>>> CreateAccount([FromBody] CreateAccountRequest data)
        {
            APIResponse<int> response = await _accountsService.CreateAccount(data);
            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }

        [HttpGet]
        [Route("{account_id}/password-variant")]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse<GetPasswordVariantResponse>>> GetPasswordVariant([FromRoute(Name = "account_id")] int accountId)
        {
            APIResponse<GetPasswordVariantResponse> response = await _accountsService.GetPasswordVariant(accountId);
            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }

        [HttpPost]
        [Route("{account_id}/authentication")]
        [AllowAnonymous]
        /*
         * Action codes:
         * 1 - Go back to client code input
         * 2 - Failed login count increment
         */
        public async Task<ActionResult<APIResponse<string>>> Authentication([FromRoute(Name = "account_id")] int accountId, [FromBody] AuthenticationRequest data)
        {
            APIResponse<string> response = await _accountsService.Authentication(accountId, data);
            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }

        #endregion
    }
}
