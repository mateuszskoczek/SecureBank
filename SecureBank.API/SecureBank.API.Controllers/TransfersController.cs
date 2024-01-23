using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureBank.API.Services;
using SecureBank.Authentication;
using SecureBank.Common;
using SecureBank.Common.Transfers;
using SecureBank.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SecureBank.API.Controllers
{
    [ApiController]
    [Route("api/transfers")]
    public class TransfersController : ControllerBase
    {
        #region SERVICES

        private ITransfersService _transfersService;

        #endregion



        #region CONSTRUCTORS

        public TransfersController(ITransfersService transfersService)
        {
            _transfersService = transfersService;
        }

        #endregion



        #region METHODS

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<APIResponse<IEnumerable<TransferResponse>>>> GetTransfers()
        {
            APIResponse<IEnumerable<TransferResponse>> response = await _transfersService.GetTransfers(new Claims(User.Claims));
            return response.Status switch
            {
                ResponseStatus.Ok => Ok(response),
                ResponseStatus.BadRequest => BadRequest(response),
                ResponseStatus.Unauthorized => Unauthorized(response),
            };
        }


        [HttpGet]
        [Route("{account_id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [RequiresClaim("admin", "True")]
        public async Task<ActionResult<APIResponse<IEnumerable<TransferResponse>>>> GetUserTransfers([FromRoute(Name = "account_id")]int accountId)
        {
            APIResponse<IEnumerable<TransferResponse>> response = await _transfersService.GetUserTransfers(accountId);
            return response.Status switch
            {
                ResponseStatus.Ok => Ok(response),
                ResponseStatus.BadRequest => BadRequest(response),
                ResponseStatus.Unauthorized => Unauthorized(response),
            };
        }

        [HttpPost]
        [Route("admin-transfer")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [RequiresClaim("admin", "True")]
        public async Task<ActionResult<APIResponse>> CreateAdminTransfer([FromBody]CreateAdminTransferRequest data)
        {
            APIResponse response = await _transfersService.CreateAdminTransfer(data);
            return response.Status switch
            {
                ResponseStatus.Ok => Ok(response),
                ResponseStatus.BadRequest => BadRequest(response),
                ResponseStatus.Unauthorized => Unauthorized(response),
            };
        }

        [HttpPost]
        [Route("user-transfer")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<APIResponse>> CreateUserTransfer([FromBody] CreateUserTransferRequest data)
        {
            APIResponse response = await _transfersService.CreateUserTransfer(data, new Claims(User.Claims));
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
