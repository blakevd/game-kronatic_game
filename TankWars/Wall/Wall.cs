using Newtonsoft.Json;
using System;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        [JsonProperty("wall")]
        public int wall;
        [JsonProperty("p1")]
        public Vector2D v1;
        [JsonProperty("p2")]
        public Vector2D v2;

        public Wall() { }

        public Wall(int wall, Vector2D v1, Vector2D v2)
        {
            this.wall = wall;
            this.v1 = v1;
            this.v2 = v2;
        }

        public int GetID()
        {
            return wall;
        }

        public Vector2D GetP1()
        {
            return v1;
        }

        public Vector2D GetP2()
        {
            return v2;
        }

        /// <summary>
        /// Gets the minimin and max values for the wall to use with checking collision calculations
        /// </summary>
        private void GetWallBoundries(out int minX, out int maxX, out int minY, out int maxY)
        {
            minX = 0;
            maxX = 0;
            minY = 0;
            maxY = 0;

            if (v1.GetX() <= v2.GetX()) //v1 is bigger
            {
                minX = (int)v1.GetX();
                maxX = (int)v2.GetX();
            }
            else // v2 is bigger
            {
                minX = (int)v2.GetX();
                maxX = (int)v1.GetX();
            }
            if (v1.GetY() <= v2.GetY()) // v1 is bigger
            {
                minY = (int)v1.GetY();
                maxY = (int)v2.GetY();
            }
            else // v2 is bigger
            {
                minY = (int)v2.GetY();
                maxY = (int)v1.GetY();
            }
        }

        /// <summary>
        /// check to see if a point collides within a radius
        /// </summary>
        public bool CheckCollision(Vector2D P, double R)
        {
            int minX, maxX, minY, maxY;

            // get the boundries 
            GetWallBoundries(out minX, out maxX, out minY, out maxY);

            // see if the point is touching the radius
            return (P.GetX() >= minX - R && P.GetX() <= maxX + R) && (P.GetY() >= minY - R && P.GetY() <= maxY + R);
        }
    }
}
