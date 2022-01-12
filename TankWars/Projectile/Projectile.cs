using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        [JsonProperty(PropertyName = "proj")]
        private int proj;
        [JsonProperty(PropertyName = "loc")]
        private Vector2D loc;
        [JsonProperty(PropertyName = "dir")]
        private Vector2D dir;
        [JsonProperty(PropertyName = "died")]
        private bool died;
        [JsonProperty(PropertyName = "owner")]
        private int owner;

        private int speed;

        public Projectile() { }

        public Projectile(int proj, Vector2D loc, Vector2D dir, bool died, int owner, int ps)
        {
            this.proj = proj;
            this.loc = loc;
            this.dir = dir;
            this.died = died;
            this.owner = owner;
            this.speed = ps;
        }

        /// <summary>
        /// Updates the current projectiles collision and speed
        /// </summary>
        /// <param name="w"></param>
        public void UpdateProjectile(Dictionary<int, Wall> w)
        {
            loc += dir * speed;

            foreach (Wall wall in w.Values)
            {
                if (wall.CheckCollision(loc, 26)) // check to see if proj is touching a wall
                {
                    Die();
                    break;
                }
            }
        }

        public int GetOwner()
        {
            return owner;
        }

        public int GetID()
        {
            return proj;
        }

        public void Die()
        {
            died = true;
        }

        public bool IsDead()
        {
            return died;
        }

        public Vector2D GetLoc()
        {
            return loc;
        }

        public Vector2D GetDir()
        {
            return dir;
        }

    }
}
