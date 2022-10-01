
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorApp.Shared
{
    public class ClientPrincipal
    {
        public string? IdentityProvider { get; set; }
        public string? UserId { get; set; }
        public string? UserDetails { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<string>? UserRoles { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<CustomClaim>? Claims { get; set; }
    }

    public class CustomClaim
    {
        public string Typ { get; set; } = "";
        public string Val { get; set; } = "";
    }
}
