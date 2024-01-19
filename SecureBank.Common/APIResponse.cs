﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SecureBank.Common
{
    public class APIResponse
    {
        [JsonProperty("message")]
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonProperty("success")]
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonProperty("action_code")]
        [JsonPropertyName("action_code")]
        public int ActionCode { get; set; }
    }

    public class APIResponse<T> : APIResponse
    {
        [JsonProperty("data")]
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}
