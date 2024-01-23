using Microsoft.IdentityModel.Tokens;
using SecureBank.Database;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.API.Authentication
{
    public class AuthenticationHelper
    {
        #region SERVICES

        private DatabaseContext _database;
        private AuthenticationConfiguration _configuration;

        #endregion



        #region CONSTRUCTORS

        public AuthenticationHelper(DatabaseContext database, AuthenticationConfiguration configuration)
        {
            _database = database;
            _configuration = configuration;
        }

        #endregion



        #region METHODS

        public string GenerateToken(Guid tokenId, Account account, bool oneTimeToken = false)
        {
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(_configuration.TokenLifetime);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim("jti", tokenId.ToString()),
                    new Claim("uid", account.Id.ToString()),
                    new Claim("first_name", account.FirstName),
                    new Claim("last_name", account.LastName),
                    new Claim("exp", expirationTime.ToString()),
                    new Claim("one_time_token", oneTimeToken.ToString()),
                    new Claim("admin", account.IsAdmin.ToString()),
                }),
                Expires = expirationTime,
                Issuer = _configuration.TokenIssuer,
                Audience = _configuration.TokenAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.TokenKey)), SecurityAlgorithms.HmacSha512)
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            handler.InboundClaimTypeMap.Clear();

            SecurityToken token = handler.CreateToken(tokenDescriptor);

            return handler.WriteToken(token);
        }

        #endregion
    }
}
