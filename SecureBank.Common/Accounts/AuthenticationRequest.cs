using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SecureBank.Common.Accounts
{
    public class AuthenticationRequest
    {
        [JsonProperty("login_request_id")]
        [JsonPropertyName("login_request_id")]
        public Guid LoginRequestId { get; set; }

        [JsonProperty("password")]
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
