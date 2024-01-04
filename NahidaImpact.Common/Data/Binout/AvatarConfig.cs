using System.Text.Json.Serialization;
using NahidaImpact.Common.Data.Binout.Ability;

namespace NahidaImpact.Common.Data.Binout;
public class AvatarConfig
{
    [JsonPropertyName("abilities")]
    public List<AbilityData> Abilities { get; set; }

    public AvatarConfig()
    {
        Abilities = new();
    }
}
