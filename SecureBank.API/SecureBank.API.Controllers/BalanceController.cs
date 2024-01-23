using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureBank.API.Services;
using SecureBank.Authentication;
using SecureBank.Common;
using SecureBank.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.API.Controllers
{
    [ApiController]
    [Route("api/balance")]
    public class BalanceController : ControllerBase
    {
        #region SERVICES

        private IBalanceService _balanceService;

        #endregion



        #region CONSTRUCTORS

        public BalanceController(IBalanceService balanceService)
        {
            _balanceService = balanceService;
        }

        #endregion



        #region METHODS

        [HttpGet]
        [Route("{account_id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [RequiresClaim("admin", "True")]
        public async Task<ActionResult<APIResponse<decimal>>> GetAccountBalance([FromRoute(Name = "account_id")]int accountId)
        {
            APIResponse<decimal> response = await _balanceService.GetAccountBalance(accountId);
            return response.Status switch
            {
                ResponseStatus.Ok => Ok(response),
                ResponseStatus.BadRequest => BadRequest(response),
                ResponseStatus.Unauthorized => Unauthorized(response),
            };
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<APIResponse<decimal>>> GetBalance()
        {
            APIResponse<decimal> response = await _balanceService.GetBalance(new Claims(User.Claims));
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
