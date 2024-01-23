using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SecureBank.Common.Accounts
{
    public class CreateAccountRequest
    {
        [JsonProperty("first_name")]
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        [JsonPropertyName("last_name")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonProperty("phone_number")]
        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty("address")]
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonProperty("pesel")]
        [JsonPropertyName("pesel")]
        public string PESEL { get; set; }

        [JsonProperty("id_card_number")]
        [JsonPropertyName("id_card_number")]
        public string IdCardNumber { get; set; }
    }
}
