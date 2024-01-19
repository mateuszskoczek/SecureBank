using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SecureBank.Helpers
{
    public class AuthenticationHelper
    {
        #region METHODS

        public static IEnumerable<Claim> ParseToken(string token)
        {
            string payload = token.Split('.')[1];

            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            byte[] jsonBytes = Convert.FromBase64String(payload);
            Dictionary<string, object>? keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            if (keyValuePairs is null)
            {
                throw new Exception("Incorrect token");
            }

            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }

        #endregion
    }
}
