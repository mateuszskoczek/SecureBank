using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SecureBank.Common.Accounts
{
    public class AccountResponse
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public int Id { get; set; }

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

        [JsonProperty("iban")]
        [JsonPropertyName("iban")]
        public string IBAN { get; set; }

        [JsonProperty("card_number")]
        [JsonPropertyName("card_number")]
        public string CardNumber { get; set; }

        [JsonProperty("card_expiration_date")]
        [JsonPropertyName("card_expiration_date")]
        public string CardExpirationDate { get; set; }

        [JsonProperty("card_cvv")]
        [JsonPropertyName("card_cvv")]
        public string CardCVV { get; set; }

        [JsonProperty("is_admin")]
        [JsonPropertyName("is_admin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("login_failed_count")]
        [JsonPropertyName("login_failed_count")]
        public int LoginFailedCount { get; set; }

        [JsonProperty("temporary_password")]
        [JsonPropertyName("temporary_password")]
        public bool TemporaryPassword { get; set; }

        [JsonProperty("lock_reason")]
        [JsonPropertyName("lock_reason")]
        public string? LockReason { get; set; }
    }
}
