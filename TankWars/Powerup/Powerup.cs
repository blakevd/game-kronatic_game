using System;
using Newtonsoft.Json;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Powerup
    {
        [JsonProperty(PropertyName = "power")]
        public int power;
        [JsonProperty(PropertyName = "loc")]
        public Vector2D loc;
        [JsonProperty(PropertyName = "died")]
        public bool died;

        public Powerup() { }

        public Powerup(int p, Vector2D l) {
            power = p;
            loc = l ;
        }

        public int GetID()
        {
            return power;
        }

        public Vector2D GetLoc()
        {
            return loc;
        }

        public bool IsDead()
        {
            return died;
        }

        public void Die()
        {
            died = true;
        }
    }
}
