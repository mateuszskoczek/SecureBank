using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureBank.API.Helpers;
using SecureBank.API.Authentication;
using SecureBank.Common;
using SecureBank.Common.Accounts;
using SecureBank.Database;
using SecureBank.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.API.Services
{
    public interface IAccountsService
    {
        Task<APIResponse<int>> CreateAccount(CreateAccountRequest data);
        Task<APIResponse<GetPasswordVariantResponse>> GetPasswordVariant(int accountId);
        Task<APIResponse<string>> Authentication(int accountId, AuthenticationRequest data);
    }



    public class AccountsService : IAccountsService
    {
        #region SERVICES

        private AuthenticationHelper _authenticationHelper;

        private DatabaseContext _database;

        private ILogger<AccountsService> _logger;

        #endregion



        #region CONSTRUCTORS

        public AccountsService(AuthenticationHelper authenticationHelper, DatabaseContext database, ILogger<AccountsService> logger)
        {
            _authenticationHelper = authenticationHelper;

            _database = database;

            _logger = logger;
        }

        #endregion



        #region PUBLIC METHODS

        public async Task<APIResponse<int>> CreateAccount(CreateAccountRequest data)
        {
            Check<CreateAccountRequest>[] checks = new Check<CreateAccountRequest>[]
            {
                new Check<CreateAccountRequest>
                {
                    CheckAction = new Predicate<CreateAccountRequest>((x) => x is null),
                    Message = "Body cannot be empty"
                },
                new Check<CreateAccountRequest>
                {
                    CheckAction = new Predicate<CreateAccountRequest>((x) => string.IsNullOrWhiteSpace(x.FirstName)),
                    Message = "First name cannot be empty"
                },
                new Check<CreateAccountRequest>
                {
                    CheckAction = new Predicate<CreateAccountRequest>((x) => string.IsNullOrWhiteSpace(x.LastName)),
                    Message = "Last name cannot be empty"
                },
                new Check<CreateAccountRequest>
                {
                    CheckAction = new Predicate<CreateAccountRequest>((x) => string.IsNullOrWhiteSpace(x.Email)),
                    Message = "Email cannot be empty"
                },
                new Check<CreateAccountRequest>
                {
                    CheckAction = new Predicate<CreateAccountRequest>((x) =>
                    {
                        try
                        {
                            MailAddress m = new MailAddress(x.Email);
                        }
                        catch (FormatException ex)
                        {
                            return true;
                        }
                        return false;
                    }),
                    Message = "Invalid email"
                },
                new Check<CreateAccountRequest>
                {
                    CheckAction = new Predicate<CreateAccountRequest>((x) => string.IsNullOrWhiteSpace(x.PhoneNumber)),
                    Message = "Phone number cannot be empty"
                },
            };

            foreach (Check<CreateAccountRequest> check in checks)
            {
                if (check.CheckAction.Invoke(data))
                {
                    return new APIResponse<int>
                    {
                        Message = check.Message,
                        Success = false
                    };
                }
            }

            Account account = new Account
            {
                FirstName = data.FirstName,
                LastName = data.LastName,
                Email = data.Email,
                PhoneNumber = data.PhoneNumber.Replace(" ", string.Empty),
            };
            await _database.Accounts.AddAsync(account);
            await _database.SaveChangesAsync();

            string password = GeneratePassword();

            await GeneratePasswordVariants(password, account.Id);

            //Send client code and temporary password to client by mail
            _logger.LogInformation($"INFO DIRECTLY TO CLIENT: Your client code is {account.Id:00000000}. Your temporary password is {password}. You will be prompted to change it at first login");

            return new APIResponse<int> 
            {
                Data = account.Id,
                Success = true 
            };
        }

        public async Task<APIResponse<GetPasswordVariantResponse>> GetPasswordVariant(int accountId)
        {
            Account? account = await _database.Accounts.FirstOrDefaultAsync(x => x.Id == accountId);
            if (account is null)
            {
                return new APIResponse<GetPasswordVariantResponse>
                {
                    Success = false,
                    Message = $"Account does not exists"
                };
            }

            if (account.LoginFailedCount >= 3)
            {
                return new APIResponse<GetPasswordVariantResponse>
                {
                    Success = false,
                    Message = $"The number of failed login attempts for this account has exceeded 3. Contact your bank to confirm your identity and unlock your account."
                };
            }

            if (account.LockReason is not null)
            {
                return new APIResponse<GetPasswordVariantResponse>
                {
                    Success = false,
                    Message = $"Account is locked. Contact your bank to confirm your identity and unlock your account."
                };
            }

            IEnumerable<AccountPassword> accountPasswords = await _database.AccountPasswords.Where(x => x.AccountId == accountId).ToArrayAsync();
            int randomIndex = Random.Shared.Next(0, accountPasswords.Count());
            AccountPassword passwordVariant = accountPasswords.ElementAt(randomIndex);

            AccountPasswordIndex[] indexes = await _database.AccountPasswordIndexes.Where(x => x.AccountPasswordId == passwordVariant.Id).ToArrayAsync();

            DateTime validTo = DateTime.Now.AddMinutes(5);
            AccountLoginRequest loginRequest = new AccountLoginRequest
            {
                AccountPasswordId = passwordVariant.Id,
                ValidTo = validTo,
            };
            await _database.AccountLoginRequests.AddAsync(loginRequest);
            await _database.SaveChangesAsync();

            return new APIResponse<GetPasswordVariantResponse>
            {
                Success = true,
                Data = new GetPasswordVariantResponse
                {
                    LoginRequestId = loginRequest.Id,
                    Indexes = indexes.Select(x => (int)x.Index).ToArray(),
                    ValidTo = validTo
                }
            };
        }

        public async Task<APIResponse<string>> Authentication(int accountId, AuthenticationRequest data)
        {
            Account? account = await _database.Accounts.FirstOrDefaultAsync(x => x.Id == accountId);

            if (account is null)
            {
                return new APIResponse<string>
                {
                    Success = false,
                    Message = $"Account does not exists"
                };
            }

            AccountLoginRequest? loginRequest = await _database.AccountLoginRequests.FirstOrDefaultAsync(x => x.Id == data.LoginRequestId);

            if (loginRequest is null)
            {
                return new APIResponse<string>
                {
                    Success = false,
                    Message = $"Login request does not exist"
                };
            }

            AccountPassword password = loginRequest.AccountPassword;

            Account loginRequestAccount = password.Account;

            if (loginRequestAccount.Id != account.Id)
            {
                account.LockReason = "Suspicious login attempt. The account provided does not match the account to which the login request is assigned.";
                loginRequestAccount.LockReason = "Suspicious login attempt. The account provided does not match the account to which the login request is assigned.";
                await _database.SaveChangesAsync();

                return new APIResponse<string>
                {
                    Success = false,
                    Message = $"Suspicious activity was detected during login. The account provided does not match the account to which the login request is assigned. Both accounts have been blocked. Contact your bank to confirm your identity and unlock your account."
                };
            }

            if (account.LockReason is not null)
            {
                return new APIResponse<string>
                {
                    Success = false,
                    Message = $"Account is locked. Contact your bank to confirm your identity and unlock your account."
                };
            }

            if (loginRequest.ValidTo < DateTime.Now)
            {
                return new APIResponse<string>
                {
                    Success = false,
                    ActionCode = 1,
                    Message = $"Login request has expired. Go back and try again."
                };
            }

            byte[] passwordDb = password.Password;
            byte[] passwordProvided = HashPassword(data.Password, password.LeftSalt, password.RightSalt);

            if (Enumerable.SequenceEqual(passwordDb, passwordProvided))
            {
                account.LoginFailedCount++;
                await _database.SaveChangesAsync();

                return new APIResponse<string>
                {
                    Success = false,
                    ActionCode = 2,
                    Message = $"Incorrect password"
                };
            }

            string token = _authenticationHelper.GenerateToken(Guid.NewGuid(), account.Id, account.TemporaryPassword);

            return new APIResponse<string>
            {
                Data = token,
                Success = true,
            };
        }

        #endregion



        #region PRIVATE METHODS

        protected byte[] HashPassword(string password, string leftSalt, string rightSalt)
        {
            SHA512 sha = SHA512.Create();
            string toHash = password;
            for (int c = 0; c < 100; c++)
            {
                string before = $"{leftSalt}{toHash}{rightSalt}";
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(before));
                toHash = Encoding.UTF8.GetString(bytes);
            }
            return Encoding.UTF8.GetBytes(toHash);
        }

        protected async Task GeneratePasswordVariants(string password, int accountId)
        {
            int charCount = password.Length / 2;

            IEnumerable<int> charIndexes = Enumerable.Range(0, password.Length);

            IEnumerable<IEnumerable<int>> indexesVariants = charIndexes.GetCombinations(charCount).OrderBy(x => Random.Shared.Next()).Take(50);

            foreach (IEnumerable<int> indexes in indexesVariants)
            {
                List<char> chars = new List<char>();
                foreach (int i in indexes)
                {
                    chars.Add(password[i]);
                }
                string leftSalt = StringExtensions.CreateRandom(20);
                string rightSalt = StringExtensions.CreateRandom(20);
                string toHash = string.Join(string.Empty, chars);
                byte[] hashed = HashPassword(toHash, leftSalt, rightSalt);

                AccountPassword accountPassword = new AccountPassword
                {
                    AccountId = accountId,
                    LeftSalt = leftSalt,
                    RightSalt = rightSalt,
                    Password = hashed,
                };
                await _database.AccountPasswords.AddAsync(accountPassword);
                await _database.SaveChangesAsync();

                IEnumerable<AccountPasswordIndex> indexesDB = indexes.Select(x => new AccountPasswordIndex
                {
                    AccountPasswordId = accountPassword.Id,
                    Index = (byte)x
                });
                await _database.AccountPasswordIndexes.AddRangeAsync(indexesDB);
                await _database.SaveChangesAsync();
            }
        }

        protected string GeneratePassword()
        {
            string passwordDigits = StringExtensions.CreateRandom(2, "1234567890");
            string passwordSymbols = StringExtensions.CreateRandom(2, "`~!@#$%^&*()-_=+[{]};:'\"\\|,<.>/?");
            string passwordSmall = StringExtensions.CreateRandom(2, "qwertyuiopasdfghjklzxcvbnm");
            string passwordBig = StringExtensions.CreateRandom(2, "QWERTYUIOPASDFGHJKLZXCVBNM");

            return string.Concat(passwordDigits, passwordSymbols, passwordSmall, passwordBig).Shuffle();
        }

        protected IEnumerable<string> CheckPassword(string password)
        {
            int minLength = 8;

            if (password.Length < minLength)
            {
                yield return $"Password must be at least {minLength} characters long";
            }
            if (!password.Any(x => Char.IsUpper(x)))
            {
                yield return $"Password must contain at least one uppercase character";
            }
            if (!password.Any(x => Char.IsLower(x)))
            {
                yield return $"Password must contain at least one lowercase character";
            }
            if (!password.Any(x => Char.IsDigit(x)))
            {
                yield return $"Password must contain at least one digit";
            }
            if (!password.Any(x => Char.IsSymbol(x)))
            {
                yield return $"Password must contain at least one special character";
            }
        }

        #endregion
    }
}
