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

        public string GenerateToken(Guid tokenId, int accountId, bool oneTimeToken = false)
        {
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(_configuration.TokenLifetime);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Jti, tokenId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, accountId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Exp, expirationTime.ToString()),
                    new Claim("one_time_token", oneTimeToken.ToString()),
                    new Claim("admin", "false"), //TODO: w zależności od użytkownika
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
