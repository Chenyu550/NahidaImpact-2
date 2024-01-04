using System.Text.Json.Serialization;

namespace NahidaImpact.SDK.Models;

public record GranterLoginRequest
{
    [JsonPropertyName("data")]
    public required string Data { get; set; }

    public record RequestData
    {
        [JsonPropertyName("uid")]
        public string? OpenId { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }
}