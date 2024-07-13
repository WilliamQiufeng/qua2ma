using Newtonsoft.Json;

namespace qua2ma.Malody
{
    /// <summary>
    /// </summary>
    public class MalodyTimingPoint
    {
        [JsonProperty("beat")]
        public List<int> Beat { get; set; }

        [JsonProperty("bpm")]
        public float Bpm { get; set; }
    }
}
