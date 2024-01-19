using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SecureBank.Common.Accounts
{
    public class GetPasswordVariantResponse
    {
        [JsonProperty("login_request_id")]
        [JsonPropertyName("login_request_id")]
        public Guid LoginRequestId { get; set; }

        [JsonProperty("indexes")]
        [JsonPropertyName("indexes")]
        public int[] Indexes { get; set; }

        [JsonProperty("valid_to")]
        [JsonPropertyName("valid_to")]
        public DateTime ValidTo { get; set; }
    }
}
