using Newtonsoft.Json;

namespace qua2ma.Malody
{
    /// <summary>
    /// </summary>
    public class MalodyEffectPoint
    {
        [JsonProperty("beat")]
        public List<int> Beat { get; set; }

        [JsonProperty("scroll")]
        public float? Scroll { get; set; }
    }
}
