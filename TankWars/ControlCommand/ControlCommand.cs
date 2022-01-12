using Newtonsoft.Json;
using System;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommand
    {
        [JsonProperty(PropertyName = "moving")]
        private string moving;
        [JsonProperty(PropertyName = "fire")]
        private string fire;
        [JsonProperty(PropertyName = "tdir")]
        private Vector2D tdir;

        public ControlCommand() { }

        public ControlCommand(string m, string f, Vector2D td)
        {
            moving = m;
            fire = f;
            tdir = td;
        }

        public string GetMoving()
        {
            return moving;
        }

        public string GetFire()
        {
            return fire;
        }

        public Vector2D GetTdir()
        {
            return tdir;
        }

    }
}
