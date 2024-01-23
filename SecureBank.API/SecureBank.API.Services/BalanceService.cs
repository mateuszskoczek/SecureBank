using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using SecureBank.Authentication;
using SecureBank.Common;
using SecureBank.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.API.Services
{
    public interface IBalanceService
    {
        Task<APIResponse<decimal>> GetAccountBalance(int accountId);
        Task<APIResponse<decimal>> GetBalance(Claims claims);
    }



    public class BalanceService : IBalanceService
    {
        #region SERVICES

        private DatabaseContext _database;

        #endregion



        #region CONSTRUCTORS

        public BalanceService(DatabaseContext database)
        {
            _database = database;
        }

        #endregion



        #region PUBLIC METHODS

        public async Task<APIResponse<decimal>> GetBalance(Claims claims) => await GetAccountBalance(claims.AccountId);

        public async Task<APIResponse<decimal>> GetAccountBalance(int accountId)
        {
            Account? account = await _database.Accounts.FirstOrDefaultAsync(x => x.Id == accountId);

            if (account is null)
            {
                return new APIResponse<decimal> 
                {
                    Status = ResponseStatus.BadRequest,
                    Message = "Account does not exists"
                };
            }

            string iban = account.IBAN;

            Transfer[] transfersIncoming = await _database.Transfers.Where(x => x.ReceiverAccountNumber == iban).ToArrayAsync();
            Transfer[] transfersOutcoming = await _database.Transfers.Where(x => x.SenderAccountNumber == iban).ToArrayAsync();

            return new APIResponse<decimal>
            {
                Data = 0 + transfersIncoming.Sum(x => x.Amount) - transfersOutcoming.Sum(x => x.Amount),
            };
        }

        #endregion



        #region PRIVATE METHODS



        #endregion
    }
}
