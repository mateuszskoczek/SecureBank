using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SecureBank.Common.Transfers
{
    public class CreateUserTransferRequest
    {

        [JsonProperty("receiver_account_number")]
        [JsonPropertyName("receiver_account_number")]
        public string ReceiverAccountNumber { get; set; }

        [JsonProperty("receiver_name")]
        [JsonPropertyName("receiver_name")]
        public string? ReceiverName { get; set; }

        [JsonProperty("receiver_address")]
        [JsonPropertyName("receiver_address")]
        public string? ReceiverAddress { get; set; }

        [JsonProperty("title")]
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonProperty("amount")]
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
    }
}
