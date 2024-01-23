using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SecureBank.Common.Transfers
{
    public class TransferResponse
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonProperty("sender_account_number")]
        [JsonPropertyName("sender_account_number")]
        public string SenderAccountNumber { get; set; }

        [JsonProperty("sender_name")]
        [JsonPropertyName("sender_name")]
        public string? SenderName { get; set; }

        [JsonProperty("sender_address")]
        [JsonPropertyName("sender_address")]
        public string? SenderAddress { get; set; }

        [JsonProperty("receiver_account_number")]
        [JsonPropertyName("receiver_account_number")]
        public string ReceiverAccountNumber { get; set; }

        [JsonProperty("receiver_name")]
        [JsonPropertyName("receiver_name")]
        public string? ReceiverName { get; set; }

        [JsonProperty("receiver_address")]
        [JsonPropertyName("receiver_address")]
        public string? ReceiverAddress { get; set; }

        [JsonProperty("amount")]
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("title")]
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonProperty("date")]
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
    }
}
