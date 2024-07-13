using Newtonsoft.Json;

namespace qua2ma.Malody
{
    /// <summary>
    /// </summary>
    public class MalodyFileMeta
    {
        [JsonProperty("creator")]
        public string Creator { get; set; }

        [JsonProperty("background")]
        public string Background { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("song")]
        public MalodyFileSong Song { get; set; }

        [JsonProperty("preview")]
        public int PreviewTime { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("mode")]
        public int Mode { get; set; }

        [JsonProperty("mode_ext")]
        public MalodyMetaKeymode Keymode { get; set; }
    }
}
