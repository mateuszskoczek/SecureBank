using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SecureBank.Common.Accounts
{
    public class ChangePasswordRequest
    {
        [JsonProperty("password")]
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
