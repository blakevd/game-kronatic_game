using Newtonsoft.Json;
using System;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Player
    {
        [JsonProperty("tank")]
        public int tank;

        [JsonProperty("loc")]
        public Vector2D loc;

        [JsonProperty("bdir")]
        public Vector2D bdir;

        [JsonProperty("tdir")]
        public Vector2D tdir = new Vector2D(0, -1);

        [JsonProperty("name")]
        public string name;

        [JsonProperty("hp")]
        public int hp = 1;

        [JsonProperty("score")]
        public int score = 0;

        [JsonProperty("died")]
        public bool died = false;

        [JsonProperty("dc")]
        public bool dc = false;

        [JsonProperty("join")]
        public bool join = false;

        public Player()
        { 
        }
    }
}
