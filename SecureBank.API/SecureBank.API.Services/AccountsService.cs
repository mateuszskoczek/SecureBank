using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecureBank.API.Helpers;
using SecureBank.API.Authentication;
using SecureBank.Authentication;
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
using SecureBank.API.Encryption;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Identity.Client;

namespace SecureBank.API.Services
{
    public interface IAccountsService
    {
        Task<APIResponse<int>> CreateAccount(CreateAccountRequest data);
        Task<APIResponse<GetPasswordVariantResponse>> GetPasswordVariant(int accountId);
        Task<APIResponse<string>> Authentication(AuthenticationRequest data);
        Task<APIResponse<string>> AuthenticationRefresh(Claims claims);
        Task<APIResponse> ChangePassword(Claims claims, ChangePasswordRequest data);
        Task<APIResponse<IEnumerable<AccountResponse>>> GetAccounts(string? iban, int? id, Claims claims);
        Task<APIResponse> ResetPassword(int accountId);
        Task<APIResponse> UnlockAccount(int accountId);
    }



    public class AccountsService : IAccountsService
    {
        #region SERVICES

        private AuthenticationHelper _authenticationHelper;

        private EncryptionHelper _encryptionHelper;

        private DatabaseContext _database;

        private ILogger<AccountsService> _logger;

        #endregion



        #region CONSTRUCTORS

        public AccountsService(AuthenticationHelper authenticationHelper, EncryptionHelper encryptionHelper, DatabaseContext database, ILogger<AccountsService> logger)
        {
            _authenticationHelper = authenticationHelper;
            _encryptionHelper = encryptionHelper;
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
                new Check<CreateAccountRequest>
                {
                    CheckAction = new Predicate<CreateAccountRequest>((x) => string.IsNullOrWhiteSpace(x.Address)),
                    Message = "Address cannot be empty"
                },
                new Check<CreateAccountRequest>
                {
                    CheckAction = new Predicate<CreateAccountRequest>((x) => string.IsNullOrWhiteSpace(x.PESEL)),
                    Message = "PESEL cannot be empty"
                },
                new Check<CreateAccountRequest>
                {
                    CheckAction = new Predicate<CreateAccountRequest>((x) => x.PESEL.Length != 11),
                    Message = "PESEL must be 11 charaters long"
                },
                new Check<CreateAccountRequest>
                {
                    CheckAction = new Predicate<CreateAccountRequest>((x) => string.IsNullOrWhiteSpace(x.IdCardNumber)),
                    Message = "Id card number cannot be empty"
                },
                new Check<CreateAccountRequest>
                {
                    CheckAction = new Predicate<CreateAccountRequest>((x) => x.IdCardNumber.Length != 9),
                    Message = "Id card number must be 9 characters long"
                },
            };

            foreach (Check<CreateAccountRequest> check in checks)
            {
                if (check.CheckAction.Invoke(data))
                {
                    return new APIResponse<int>
                    {
                        Message = check.Message,
                        Status = ResponseStatus.BadRequest,
                    };
                }
            }

            byte[] pesel = _encryptionHelper.Encrypt(data.PESEL);
            byte[] idCardNumber = _encryptionHelper.Encrypt(data.IdCardNumber);
            byte[] cardCVV = _encryptionHelper.Encrypt(StringExtensions.CreateRandom(3, "1234567890"));
            byte[] cardExpirationDate = _encryptionHelper.Encrypt(DateTime.Now.AddYears(5).ToString("MM/yy"));

            Account account = new Account
            {
                FirstName = data.FirstName,
                LastName = data.LastName,
                Email = data.Email,
                PhoneNumber = data.PhoneNumber.Replace(" ", string.Empty),
                Address = data.Address,
                PESEL = pesel,
                IdCardNumber = idCardNumber,
                IBAN = string.Empty,
                CardNumber = new byte[0],
                CardCVV = cardCVV,
                CardExpirationDate = cardExpirationDate
            };
            await _database.Accounts.AddAsync(account);
            await _database.SaveChangesAsync();


            string ibanGen = $"549745{StringExtensions.CreateRandom(12, "1234567890")}{account.Id:00000000}";

            string cardNumberGen = $"49{StringExtensions.CreateRandom(6, "1234567890")}{account.Id:00000000}";
            byte[] cardNumber = _encryptionHelper.Encrypt(cardNumberGen);

            account.IBAN = ibanGen;
            account.CardNumber = cardNumber;

            await _database.SaveChangesAsync();

            string password = GeneratePassword();

            await GeneratePasswordVariants(password, account.Id);

            //Send client code and temporary password to client by mail
            _logger.LogInformation($"INFO DIRECTLY TO CLIENT: Your client code is {account.Id:00000000}. Your temporary password is {password}. You will be prompted to change it at first login");

            return new APIResponse<int>
            {
                Data = account.Id
            };
        }

        public async Task<APIResponse<GetPasswordVariantResponse>> GetPasswordVariant(int accountId)
        {
            Account? account = await _database.Accounts.FirstOrDefaultAsync(x => x.Id == accountId);
            if (account is null)
            {
                return new APIResponse<GetPasswordVariantResponse>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = $"Account does not exists"
                };
            }

            if (account.LoginFailedCount >= 3)
            {
                return new APIResponse<GetPasswordVariantResponse>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = $"The number of failed login attempts for this account has exceeded 3. Contact your bank to confirm your identity and unlock your account."
                };
            }

            if (account.LockReason is not null)
            {
                return new APIResponse<GetPasswordVariantResponse>
                {
                    Status = ResponseStatus.BadRequest,
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
                Data = new GetPasswordVariantResponse
                {
                    LoginRequestId = loginRequest.Id,
                    Indexes = indexes.Select(x => (int)x.Index).ToArray(),
                    ValidTo = validTo
                }
            };
        }

        public async Task<APIResponse<string>> Authentication(AuthenticationRequest data)
        {
            AccountLoginRequest? loginRequest = await _database.AccountLoginRequests.FirstOrDefaultAsync(x => x.Id == data.LoginRequestId);

            if (loginRequest is null)
            {
                return new APIResponse<string>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = $"Login request does not exist"
                };
            }

            AccountPassword password = loginRequest.AccountPassword;

            Account loginRequestAccount = password.Account;

            APIResponse<string>? accountCheck = CheckAccount(loginRequestAccount);
            if (accountCheck is not null)
            {
                return accountCheck;
            }

            if (loginRequest.ValidTo < DateTime.Now)
            {
                return new APIResponse<string>
                {
                    Status = ResponseStatus.BadRequest,
                    ActionCode = 1,
                    Message = $"Login request has expired. Go back and try again."
                };
            }

            byte[] passwordDb = password.Password;
            byte[] passwordProvided = HashPassword(data.Password, password.LeftSalt, password.RightSalt);

            if (!Enumerable.SequenceEqual(passwordDb, passwordProvided))
            {
                loginRequestAccount.LoginFailedCount++;
                await _database.SaveChangesAsync();

                return new APIResponse<string>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = $"Incorrect password"
                };
            }

            loginRequestAccount.LoginFailedCount = 0;
            await _database.SaveChangesAsync();

            string token = _authenticationHelper.GenerateToken(Guid.NewGuid(), loginRequestAccount, loginRequestAccount.TemporaryPassword);

            return new APIResponse<string>
            { 
                ActionCode = loginRequestAccount.TemporaryPassword ? 2 : 0,
                Data = token
            };
        }

        public async Task<APIResponse<string>> AuthenticationRefresh(Claims claims)
        {
            if (claims.IsOneTimeToken)
            {
                return new APIResponse<string>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = $"One time token cannot be refreshed."
                };
            }

            Account? account = await _database.Accounts.FirstOrDefaultAsync(x => x.Id == claims.AccountId);

            APIResponse<string>? accountCheck = CheckAccount(account);
            if (accountCheck is not null)
            {
                return accountCheck;
            }

            string token = _authenticationHelper.GenerateToken(Guid.NewGuid(), account, false);

            return new APIResponse<string>
            {
                Data = token
            };
        }

        public async Task<APIResponse> ChangePassword(Claims claims, ChangePasswordRequest data)
        {
            string password = data.Password;
            Account? account = await _database.Accounts.FirstOrDefaultAsync(x => x.Id == claims.AccountId);

            if (account is null)
            {
                return new APIResponse<string>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = $"Account does not exists"
                };
            }

            IEnumerable<string> passwordChecks = CheckPassword(password);
            if (passwordChecks.Any())
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Provided password does not meet the security requirements:");
                foreach (string check in passwordChecks)
                {
                    sb.AppendLine(check);
                }
                return new APIResponse
                {
                    Status = ResponseStatus.BadRequest,
                    Message = sb.ToString()
                };
            }

            IEnumerable<AccountPasswordIndex> indexes = await _database.AccountPasswordIndexes.Where(x => x.AccountPassword.AccountId == claims.AccountId).ToListAsync();
            _database.AccountPasswordIndexes.AttachRange(indexes);
            _database.AccountPasswordIndexes.RemoveRange(indexes);
            await _database.SaveChangesAsync();

            IEnumerable<AccountPassword> variants = await _database.AccountPasswords.Where(x => x.AccountId == claims.AccountId).ToListAsync();
            _database.AccountPasswords.AttachRange(variants);
            _database.AccountPasswords.RemoveRange(variants);
            await _database.SaveChangesAsync();

            await GeneratePasswordVariants(password, claims.AccountId);

            account.TemporaryPassword = false;
            await _database.SaveChangesAsync();

            return new APIResponse();
        }

        public async Task<APIResponse<IEnumerable<AccountResponse>>> GetAccounts(string? iban, int? id, Claims claims)
        {
            IEnumerable<Account> accounts = await _database.Accounts.ToListAsync();
            if (id is not null)
            {
                accounts = accounts.Where(x => x.Id == id);
            }
            if (iban is not null)
            {
                accounts = accounts.Where(x => x.IBAN == iban);
            }

            if (accounts.Any(x => x.Id != claims.AccountId) && !claims.IsAdmin) 
            {
                return new APIResponse<IEnumerable<AccountResponse>>
                {
                    Status = ResponseStatus.Unauthorized,
                    Message = $"You don't have permission to get information about accounts that aren't yours"
                };
            }

            List<AccountResponse> data = new List<AccountResponse>();
            foreach (Account account in accounts)
            {
                data.Add(new AccountResponse
                {
                    Id = account.Id,
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                    Email = account.Email,
                    PhoneNumber = account.PhoneNumber,
                    Address = account.Address,
                    PESEL = _encryptionHelper.Decrypt(account.PESEL),
                    IdCardNumber = _encryptionHelper.Decrypt(account.IdCardNumber),
                    IBAN = account.IBAN,
                    CardNumber = _encryptionHelper.Decrypt(account.CardNumber),
                    CardExpirationDate = _encryptionHelper.Decrypt(account.CardExpirationDate),
                    CardCVV = _encryptionHelper.Decrypt(account.CardCVV),
                    IsAdmin = account.IsAdmin,
                    LoginFailedCount = account.LoginFailedCount,
                    TemporaryPassword = account.TemporaryPassword,
                    LockReason = account.LockReason,
                });
            }

            return new APIResponse<IEnumerable<AccountResponse>>
            {
                Data = data
            };
        }

        public async Task<APIResponse> ResetPassword(int accountId)
        {
            Account? account = await _database.Accounts.FirstOrDefaultAsync(x => x.Id == accountId);

            if (account is null)
            {
                return new APIResponse<string>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = $"Account does not exists"
                };
            }

            await PasswordReset(account);

            return new APIResponse<int>
            {
                Data = account.Id
            };
        }

        public async Task<APIResponse> UnlockAccount(int accountId)
        {
            Account? account = await _database.Accounts.FirstOrDefaultAsync(x => x.Id == accountId);

            if (account is null)
            {
                return new APIResponse<string>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = $"Account does not exists"
                };
            }

            await PasswordReset(account);

            account.LockReason = null;
            account.LoginFailedCount = 0;
            await _database.SaveChangesAsync();

            return new APIResponse<int>
            {
                Data = account.Id
            };
        }

        #endregion



        #region PRIVATE METHODS

        protected async Task PasswordReset(Account account)
        {
            IEnumerable<AccountPasswordIndex> indexes = await _database.AccountPasswordIndexes.Where(x => x.AccountPassword.AccountId == account.Id).ToListAsync();
            _database.AccountPasswordIndexes.AttachRange(indexes);
            _database.AccountPasswordIndexes.RemoveRange(indexes);
            await _database.SaveChangesAsync();

            IEnumerable<AccountPassword> variants = await _database.AccountPasswords.Where(x => x.AccountId == account.Id).ToListAsync();
            _database.AccountPasswords.AttachRange(variants);
            _database.AccountPasswords.RemoveRange(variants);
            await _database.SaveChangesAsync();

            string password = GeneratePassword();

            await GeneratePasswordVariants(password, account.Id);

            account.TemporaryPassword = true;
            await _database.SaveChangesAsync();

            _logger.LogInformation($"INFO DIRECTLY TO CLIENT: Your new temporary password is {password}. You will be prompted to change it at first login");
        }

        protected APIResponse<string>? CheckAccount(Account? account)
        {
            if (account is null)
            {
                return new APIResponse<string>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = $"Account does not exists."
                };
            }

            if (account.LockReason is not null)
            {
                return new APIResponse<string>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = $"Account is locked. Contact your bank to confirm your identity and unlock your account."
                };
            }

            if (account.LoginFailedCount >= 3)
            {
                return new APIResponse<string>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = $"The number of failed login attempts for this account has exceeded 3. Contact your bank to confirm your identity and unlock your account."
                };
            }

            return null;
        }

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
            uint maxLength = 20;

            if (password.Length < minLength)
            {
                yield return $"Password must be at least {minLength} characters long";
            }
            if (password.Length > maxLength)
            {
                yield return $"Password cannot be longer than {maxLength} characters";
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
            if (!password.Any(x => !Char.IsDigit(x) && !Char.IsUpper(x) && !Char.IsLower(x)))
            {
                yield return $"Password must contain at least one special character";
            }
        }

        #endregion
    }
}
