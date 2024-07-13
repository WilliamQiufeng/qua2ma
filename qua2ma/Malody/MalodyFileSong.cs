using Newtonsoft.Json;

namespace qua2ma.Malody
{
    /// <summary>
    /// </summary>
    public class MalodyFileSong
    {
        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("artistorg")]
        public string ArtistOriginal { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("titleorg")]
        public string TitleOriginal { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
