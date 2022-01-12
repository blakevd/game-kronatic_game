using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        [JsonProperty(PropertyName = "tank")]
        private int ID;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "bdir")]
        private Vector2D orientation;

        [JsonProperty(PropertyName = "tdir")]
        private Vector2D tdir;

        [JsonProperty(PropertyName = "name")]
        private string name;

        [JsonProperty(PropertyName = "hp")]
        private int hitPoints;

        [JsonProperty(PropertyName = "score")]
        private int score;

        [JsonProperty(PropertyName = "died")]
        private bool died = false;

        [JsonProperty(PropertyName = "dc")]
        private bool disconnected = false;

        [JsonProperty(PropertyName = "join")]
        private bool joined = false;

        private string moving;
        private Vector2D moveSpeed;
        private string fire;

        private int lastDeathTime;
        private int lastShotTime;

        private int beamAmmo; // powerup ammo

        private bool disabled;

        public Tank() { }

        public Tank(int id, string name, Vector2D pos, Vector2D vel, Vector2D dir, int hp)
        {
            this.ID = id;
            this.name = name;

            this.location = pos;
            this.orientation = dir;

            this.moveSpeed = new Vector2D(0, 0);
            this.tdir = new Vector2D(0f, 1f);
            this.hitPoints = hp;
            this.score = 0;
            this.died = false;
            this.joined = false;
            this.disconnected = false;

            this.beamAmmo = 0;
            this.lastShotTime = 0;
            this.lastDeathTime = 0;

            this.moving = "none";
            this.disabled = false;
        }

        public Vector2D GetLoc()
        {
            return location;
        }

        public Vector2D GetOri()
        {
            return orientation;
        }

        public Vector2D GetTdir()
        {
            return tdir;
        }

        public int GetID()
        {
            return ID;
        }

        public void AddScore()
        {
            score++;
        }

        public int GetHP()
        {
            return hitPoints;
        }
        public bool IsDead()
        {
            return died;
        }

        public void SetDead(bool d)
        {
            died = d;
        }

        public bool IsDisconnected()
        {
            return disconnected;
        }
        public void SetTdir(Vector2D v)
        {
            tdir = v;
        }

        /// <summary>
        /// Sets the join game true for a frame
        /// </summary>
        public void JoinGame()
        {
            joined = true;
        }

        /// <summary>
        /// Finished the tank joining the frame after it joins
        /// </summary>
        public void DoneJoiningGame()
        {
            joined = false;
        }


        /// <summary>
        /// Tank wraps aroudn the map
        /// </summary>
        public void WrapLoc(int size)
        {
            if (location.GetX() > size / 2)
                location = new Vector2D(-size / 2, location.GetY());
            if (location.GetX() < -size / 2)
                location = new Vector2D(size / 2, location.GetY());
            if (location.GetY() > size / 2)
                location = new Vector2D(location.GetX(), -size / 2);
            if (location.GetY() < -size / 2)
                location = new Vector2D(location.GetX(), size / 2);

        }

        /// <summary>
        /// Updates the tanks collision and speed
        /// </summary>
        /// <param name="w"></param>
        public void UpdateTank(Dictionary<int, Wall> w)
        {
            Vector2D prevLoc = location;
            Vector2D prevSpeed = moveSpeed;

            location += moveSpeed;

            foreach (Wall wall in w.Values)
            {
                if (wall.CheckCollision(location, 51)) // check to see if tank is touching a wall
                {
                    location = prevLoc; // dont let it move through it
                    break;
                }
            }
        }

        /// <summary>
        /// Sets the tanks movement based on the given command
        /// </summary>
        public void SetMoving(string m, float speed)
        {
            moving = m;

            if (moving != "none" && !IsDisabled())
            {
                if (moving == "left")
                    orientation = new Vector2D(-1, 0);
                else if (moving == "right")
                    orientation = new Vector2D(1, 0);
                else if (moving == "up")
                    orientation = new Vector2D(0, -1);
                else if (moving == "down")
                    orientation = new Vector2D(0, 1);
            }
            else
                speed = 0;

            moveSpeed = orientation * speed;
        }

        public int GetLastShotTime()
        {
            return lastShotTime;
        }

        public string GetFire()
        {
            return fire;
        }
        public void SetFire(string f)
        {
            fire = f;
        }

        public void AddBeamAmmo()
        {
            beamAmmo++;
        }
        public int GetBeamAmmo()
        {
            return beamAmmo;
        }

        /// <summary>
        /// Make the tank shoot and lose ammo if its a beam
        /// </summary>
        /// <param name="time"></param>
        /// <param name="fire"></param>
        public void Shoot(int time, string fire)
        {
            if (fire == "alt")
            {
                beamAmmo--;
            }

            lastShotTime = time;
        }

        /// <summary>
        /// method that makes this tank take damage and kills it if its health is low enough
        /// </summary>
        public void TakeDamage(int amount, int time)
        {
            hitPoints -= amount;
            if (hitPoints <= 0)
                Die(time);
        }

        /// <summary>
        ///  Set the tanks stats so that it is dead
        /// </summary>
        public void Die(int time)
        {
            hitPoints = 0;
            died = true;
            disabled = true;
            lastDeathTime = time;
        }

        public int GetLastDeathTime()
        {
            return lastDeathTime;
        }

        /// <summary>
        /// Disconnect and kill the tank
        /// </summary>
        /// <param name="time"></param>
        public void DisconnectTank(int time)
        {
            if (!IsDead())
                Die(time);

            disconnected = true;
        }

        /// <summary>
        /// used to help respawn the tank and disable it while it is dead
        /// </summary>
        /// <returns></returns>
        public bool IsDisabled()
        {
            return disabled;
        }

        /// <summary>
        /// Respawn the tank
        /// </summary>
        public void Respawn(Vector2D randomLoc)
        {
            hitPoints = 3;
            died = false;
            disabled = false;

            location = randomLoc;
            moveSpeed = new Vector2D(0, 0);
            orientation = new Vector2D(0, 0);
        }
    }
}
