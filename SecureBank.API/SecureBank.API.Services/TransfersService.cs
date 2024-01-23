using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SecureBank.API.Helpers;
using SecureBank.Authentication;
using SecureBank.Common;
using SecureBank.Common.Accounts;
using SecureBank.Common.Transfers;
using SecureBank.Database;
using SecureBank.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.API.Services
{
    public interface ITransfersService
    {
        Task<APIResponse<IEnumerable<TransferResponse>>> GetTransfers(Claims claims);
        Task<APIResponse<IEnumerable<TransferResponse>>> GetUserTransfers(int accountId);
        Task<APIResponse> CreateAdminTransfer(CreateAdminTransferRequest data);
        Task<APIResponse> CreateUserTransfer(CreateUserTransferRequest data, Claims claims);
    }




    public class TransfersService : ITransfersService
    {
        #region SERVICES

        private DatabaseContext _database;

        #endregion



        #region CONSTRUCTORS

        public TransfersService(DatabaseContext database)
        {
            _database = database;
        }

        #endregion



        #region PUBLIC METHODS

        public async Task<APIResponse<IEnumerable<TransferResponse>>> GetTransfers(Claims claims) => await GetUserTransfers(claims.AccountId);
        public async Task<APIResponse<IEnumerable<TransferResponse>>> GetUserTransfers(int accountId)
        {
            Account? account = await _database.Accounts.FirstOrDefaultAsync(x => x.Id == accountId);

            if (account is null)
            {
                return new APIResponse<IEnumerable<TransferResponse>>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = "Account does not exists"
                };
            }

            string iban = account.IBAN;

            List<TransferResponse> list = new List<TransferResponse>();
            await foreach (Transfer transfer in _database.Transfers.Where(x => x.SenderAccountNumber == iban || x.ReceiverAccountNumber == iban).AsAsyncEnumerable())
            {
                list.Add(new TransferResponse
                {
                    Id = transfer.Id,
                    SenderAccountNumber = transfer.SenderAccountNumber,
                    SenderAddress = transfer.SenderAddress,
                    SenderName = transfer.SenderName,
                    ReceiverAccountNumber = transfer.ReceiverAccountNumber,
                    ReceiverAddress = transfer.ReceiverAddress,
                    ReceiverName = transfer.ReceiverName,
                    Amount = transfer.Amount,
                    Title = transfer.Title,
                    Date = transfer.Date,
                });
            }

            return new APIResponse<IEnumerable<TransferResponse>>
            {
                Data = list
            };
        }

        public async Task<APIResponse> CreateAdminTransfer(CreateAdminTransferRequest data)
        {
            Check<CreateAdminTransferRequest>[] checks = new Check<CreateAdminTransferRequest>[]
            {
                new Check<CreateAdminTransferRequest>
                {
                    CheckAction = new Predicate<CreateAdminTransferRequest>((x) => x is null),
                    Message = "Body cannot be empty"
                },
                new Check<CreateAdminTransferRequest>
                {
                    CheckAction = new Predicate<CreateAdminTransferRequest>((x) => x.SenderAccountNumber is null),
                    Message = "Sender account number cannot be empty"
                },
                new Check<CreateAdminTransferRequest>
                {
                    CheckAction = new Predicate<CreateAdminTransferRequest>((x) => !x.SenderAccountNumber.All(y => char.IsDigit(y))),
                    Message = "Wrong sender account number format. Account number consists only of digits"
                },
                new Check<CreateAdminTransferRequest>
                {
                    CheckAction = new Predicate<CreateAdminTransferRequest>((x) => x.SenderAccountNumber.Length != 26),
                    Message = "Sender account number cannot be empty"
                },
                new Check<CreateAdminTransferRequest>
                {
                    CheckAction = new Predicate<CreateAdminTransferRequest>((x) => x.ReceiverAccountNumber is null),
                    Message = "Receiver account number cannot be empty"
                },
                new Check<CreateAdminTransferRequest>
                {
                    CheckAction = new Predicate<CreateAdminTransferRequest>((x) => !x.ReceiverAccountNumber.All(y => char.IsDigit(y))),
                    Message = "Wrong receiver account number format. Account number consists only of digits"
                },
                new Check<CreateAdminTransferRequest>
                {
                    CheckAction = new Predicate<CreateAdminTransferRequest>((x) => x.ReceiverAccountNumber.Length != 26),
                    Message = "Receiver account number cannot be empty"
                },
                new Check<CreateAdminTransferRequest>
                {
                    CheckAction = new Predicate<CreateAdminTransferRequest>((x) => x.Amount <= 0),
                    Message = "Receiver account number cannot be empty"
                },
            };

            foreach (Check<CreateAdminTransferRequest> check in checks)
            {
                if (check.CheckAction.Invoke(data))
                {
                    return new APIResponse
                    {
                        Message = check.Message,
                        Status = ResponseStatus.BadRequest,
                    };
                }
            }

            data.Amount = Math.Round(data.Amount, 2, MidpointRounding.ToEven);

            Transfer transfer = new Transfer
            {
                SenderAccountNumber = data.SenderAccountNumber,
                SenderAddress = data.SenderAddress,
                SenderName = data.SenderName,
                ReceiverAccountNumber = data.ReceiverAccountNumber,
                ReceiverAddress = data.ReceiverAddress,
                ReceiverName = data.ReceiverName,
                Amount = data.Amount,
                Title = data.Title,
                Date = DateTime.Now,
            };
            await _database.Transfers.AddAsync(transfer);
            await _database.SaveChangesAsync();

            return new APIResponse();
        }

        public async Task<APIResponse> CreateUserTransfer(CreateUserTransferRequest data, Claims claims)
        {
            Check<CreateUserTransferRequest>[] checks = new Check<CreateUserTransferRequest>[]
            {
                new Check<CreateUserTransferRequest>
                {
                    CheckAction = new Predicate<CreateUserTransferRequest>((x) => x is null),
                    Message = "Body cannot be empty"
                },
                new Check<CreateUserTransferRequest>
                {
                    CheckAction = new Predicate<CreateUserTransferRequest>((x) => x.ReceiverAccountNumber is null),
                    Message = "Receiver account number cannot be empty"
                },
                new Check<CreateUserTransferRequest>
                {
                    CheckAction = new Predicate<CreateUserTransferRequest>((x) => !x.ReceiverAccountNumber.All(y => char.IsDigit(y))),
                    Message = "Wrong receiver account number format. Account number consists only of digits"
                },
                new Check<CreateUserTransferRequest>
                {
                    CheckAction = new Predicate<CreateUserTransferRequest>((x) => x.ReceiverAccountNumber.Length != 26),
                    Message = "Receiver account number cannot be empty"
                },
                new Check<CreateUserTransferRequest>
                {
                    CheckAction = new Predicate<CreateUserTransferRequest>((x) => x.Amount <= 0),
                    Message = "Receiver account number cannot be empty"
                },
            };

            foreach (Check<CreateUserTransferRequest> check in checks)
            {
                if (check.CheckAction.Invoke(data))
                {
                    return new APIResponse
                    {
                        Message = check.Message,
                        Status = ResponseStatus.BadRequest,
                    };
                }
            }

            Account? account = await _database.Accounts.FirstOrDefaultAsync(x => x.Id == claims.AccountId);

            if (account is null)
            {
                return new APIResponse<IEnumerable<TransferResponse>>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = "Account does not exists"
                };
            }

            data.Amount = Math.Round(data.Amount, 2, MidpointRounding.ToEven);

            Transfer transfer = new Transfer
            {
                SenderAccountNumber = account.IBAN,
                SenderAddress = account.Address,
                SenderName = $"{account.FirstName} {account.LastName}",
                ReceiverAccountNumber = data.ReceiverAccountNumber,
                ReceiverAddress = data.ReceiverAddress,
                ReceiverName = data.ReceiverName,
                Amount = data.Amount,
                Title = data.Title,
                Date = DateTime.Now,
            };
            await _database.Transfers.AddAsync(transfer);
            await _database.SaveChangesAsync();

            return new APIResponse();
        }

        #endregion
    }
}
