using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.Authentication
{
    public class Claims
    {
        #region PROPERTIES

        public Guid Id { get; protected set; }
        public int AccountId { get; protected set; }
        public string FirstName { get; protected set; }
        public string LastName { get; protected set; }
        public DateTime ExpirationTime { get; protected set; }
        public bool IsOneTimeToken { get; protected set; }
        public bool IsAdmin { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        public Claims(IEnumerable<Claim> claims)
        {
            Id = Guid.Parse(claims.Where(x => x.Type == "jti").First().Value);
            AccountId = int.Parse(claims.Where(x => x.Type == "uid").First().Value);
            FirstName = claims.Where(x => x.Type == "first_name").First().Value;
            LastName = claims.Where(x => x.Type == "last_name").First().Value;
            ExpirationTime = new DateTime(long.Parse(claims.Where(x => x.Type == "exp").First().Value));
            IsOneTimeToken = bool.Parse(claims.Where(x => x.Type == "one_time_token").First().Value);
            IsAdmin = bool.Parse(claims.Where(x => x.Type == "admin").First().Value);
        }

        #endregion
    }
}
