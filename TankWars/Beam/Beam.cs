using System;
using Newtonsoft.Json;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Beam
    {
        [JsonProperty(PropertyName = "beam")]
        private int ID;
        [JsonProperty(PropertyName = "org")]
        private Vector2D origin;
        [JsonProperty(PropertyName = "dir")]
        private Vector2D dir;
        [JsonProperty(PropertyName = "owner")]
        private int owner;

        public Beam() { }
        public Beam(int i, Vector2D o, Vector2D d, int p)
        {
            ID = i;
            origin = o;
            dir = d;
            owner = p;
        }

        public int GetID()
        {
            return ID;
        }

        public Vector2D GetOrigin()
        {
            return origin;
        }

        public Vector2D GetDir()
        {
            return dir;
        }

        public int GetOwner()
        {
            return owner;
        }
    }
}
