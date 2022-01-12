using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankWars
{
    public class World
    {

        public Dictionary<int, Tank> Tanks;
        public Dictionary<int, Powerup> Powerups;
        public Dictionary<int, Wall> Walls;
        public Dictionary<int, Projectile> Projectiles;
        public Dictionary<int, Beam> Beams;

        private static int gameTime;
        private static int projID;
        private static int powID;

        // XML vars
        private static int framesPerShot;
        private static int respawnRate;
        private static float engineStrength;
        private static int startingHitPoints;
        private static int projectileSpeed;

        // powerup vars
        private static int maxPowerups;
        private static int totalPowerups;
        private static int prevPowerupTime;
        private static int powerupDelay;

        public int size
        { get; private set; }

        public World(int _size, Stack<Wall> newWalls, int fps, int respawnTime, float eStrength, int mpu, int mpud, int hp, int projSpeed)
        {
            Tanks = new Dictionary<int, Tank>();
            Powerups = new Dictionary<int, Powerup>();
            Projectiles = new Dictionary<int, Projectile>();
            Walls = new Dictionary<int, Wall>();
            Beams = new Dictionary<int, Beam>();
            size = _size;

            framesPerShot = fps;
            respawnRate = respawnTime;
            engineStrength = eStrength;
            maxPowerups = mpu;
            powerupDelay = mpud;
            startingHitPoints = hp;
            projectileSpeed = projSpeed;

            prevPowerupTime = 0;
            totalPowerups = 0;

            projID = 0;
            powID = 0;
            gameTime = 0;

            // add all our walls to the dict
            foreach (Wall w in newWalls)
            {
                Walls.Add(w.GetID(), w);
            }
        }

        /// <summary>
        /// private helper method to check collision between point P and D+R where r is the radius
        /// </summary>
        /// <returns></returns>
        private bool CheckCollisionRadius(Vector2D P,Vector2D D, int R)
        {
            return (P.GetX() >= D.GetX() - R && P.GetX() <= D.GetX() + R) && (P.GetY() >= D.GetY() - R && P.GetY() <= D.GetY() + R);
        }

        /// <summary>
        /// Method to update everything happening in the world
        /// </summary>
        public void UpdateTheWorld()
        {
            // update poewrups
            if (gameTime - prevPowerupTime > powerupDelay)
            {
                if (totalPowerups <= maxPowerups)
                {
                    Powerup pow = new Powerup(powID, GetRandomTankLoc());
                    Powerups.Add(pow.GetID(), pow);
                    powID++;
                    totalPowerups++;
                }
            }
            // update tanks
            foreach (Tank tank in Tanks.Values)
            {
                // respawn the tank if it is dead
                if (tank.IsDisabled() && gameTime - tank.GetLastDeathTime() > respawnRate)
                {
                    tank.Respawn(GetRandomTankLoc());
                }

                tank.UpdateTank(Walls);
                tank.WrapLoc(size);

                foreach (Powerup pow in Powerups.Values)
                {
                    if (!pow.IsDead())
                    {
                        Vector2D P = pow.GetLoc();
                        if (CheckCollisionRadius(P, tank.GetLoc(), 25)) // check if it collides with the point of the powerup
                        {
                            tank.AddBeamAmmo();
                            pow.Die();
                        }
                    }
                }
            }

            // update projectiles and there hits
            foreach (Projectile proj in Projectiles.Values)
            {
                proj.UpdateProjectile(Walls); // update proj speed

                // kill proj if its out of bounds
                if (proj.GetLoc().GetX() < -size / 2 || proj.GetLoc().GetX() > size / 2 || proj.GetLoc().GetY() < -size / 2 || proj.GetLoc().GetY() > size / 2) // remove from dict
                    proj.Die();

                foreach (Tank tank in Tanks.Values) // check hit detection
                {
                    if (!proj.IsDead() && !tank.IsDisabled() && proj.GetOwner() != tank.GetID())
                    {
                        Vector2D P = proj.GetLoc();
                        int R = 25; // radius of the tank which is 50/2
                        if (CheckCollisionRadius(P, tank.GetLoc(), R)) // check if it hits
                        {
                            proj.Die(); // delete the proj

                            if (Tanks.ContainsKey(proj.GetOwner())) // see if hit person is playing and update damage
                                tank.TakeDamage(1, gameTime);

                            if (Tanks.ContainsKey(tank.GetID()) && tank.GetHP() <= 0) // see if owner is still playing and add score
                            {
                                Tanks[proj.GetOwner()].AddScore();
                            }
                        }
                    }
                }
            }
            // update beams and collisions for it
            foreach (Beam b in Beams.Values)
            {
                foreach (Tank tank in Tanks.Values)
                {
                    if (!tank.IsDisabled() && b.GetID() != tank.GetID() && Intersects(b.GetOrigin(), b.GetDir(), tank.GetLoc(), 25))
                    {
                        tank.TakeDamage(tank.GetHP(), gameTime);
                    }
                }
            }

            // update the game tick to use as a timer
            gameTime++;
        }

        /// <summary>
        /// Determines if a ray interescts a circle
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        public static bool Intersects(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = rayDir.Dot(rayDir);
            double b = ((rayOrig - center) * 2.0).Dot(rayDir);
            double c = (rayOrig - center).Dot(rayOrig - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }

        /// <summary>
        /// Method that is called after the world updates to clean up dead proj, etc.
        /// </summary>
        public void RemoveDeadStuff()
        {
            // delete disconnected tanks
            // but dont remove if they are dead because we need to respawn them
            foreach (Tank tank in Tanks.Values)
            {
                if (Tanks.ContainsKey(tank.GetID()))
                {
                    tank.DoneJoiningGame();
                    tank.SetDead(false);

                    if (tank.IsDisconnected())
                        Tanks.Remove(tank.GetID());
                }
            }

            // delete dead projectiles
            foreach (Projectile proj in Projectiles.Values)
            {
                if (proj.IsDead())
                    Projectiles.Remove(proj.GetID());
            }

            /// delete dead powerups
            foreach (Powerup pow in Powerups.Values)
                if (pow.IsDead())
                    Powerups.Remove(pow.GetID());

            // delete all beams as they only last for a frame
            Beams.Clear();
        }

        /// <summary>
        /// method to update the server with commands from the client
        /// </summary>
        public void UpdateTankWithCommand(Tank tank, string cmd)
        {
            if (cmd == "" || !cmd.StartsWith("{"))
                return;

            try
            {
                ControlCommand command = JsonConvert.DeserializeObject<ControlCommand>(cmd); // parse our cmd
                string moving = command.GetMoving();
                string fire = command.GetFire();
                Vector2D tdir = command.GetTdir();

                // set tanks values to above stuff
                tank.SetMoving(moving, engineStrength);
                tank.SetFire(fire);
                tank.SetTdir(tdir);

                Shoot(tank, gameTime, fire);
            }
            catch (Exception e)
            {
                return;
            }
        }

        /// <summary>
        /// private helper to tell a tank to shoot either a proj or a beam if they are allowed to
        /// </summary>
        private void Shoot(Tank tank, int time, string fire)
        {
            if (tank.IsDead() || tank.IsDisabled())
                return;

            if (fire == "main")
            {
                if (time - tank.GetLastShotTime() > framesPerShot) // shoot regular proj 
                {
                    tank.Shoot(time, fire);
                    Projectile proj = new Projectile(projID, tank.GetLoc(), tank.GetTdir(), false, tank.GetID(), projectileSpeed);
                    projID++;

                    Projectiles.Add(proj.GetID(), proj); // add to the worlds proj
                }
            }
            else if (fire == "alt")
            {
                if ((time - tank.GetLastShotTime() > framesPerShot) && tank.GetBeamAmmo() > 0) // shoot regular proj 
                {
                    tank.Shoot(time, fire);
                    Beam beam = new Beam(projID, tank.GetLoc(), tank.GetTdir(), tank.GetID());
                    projID++;

                    Beams.Add(beam.GetID(), beam);
                }
            }
        }

        /// <summary>
        /// Gets a random location on the map that is not colliding with any walls and returns it
        /// </summary>
        /// <returns></returns>
        public Vector2D GetRandomTankLoc()
        {
            Vector2D randLoc;
            while (true)
            {
                Random r = new Random();
                int x = r.Next(-size / 2, size / 2);
                int y = r.Next(-size / 2, size / 2);

                Vector2D point = new Vector2D(x, y);

                // make sure the point is not touching any walls
                bool touched = false;
                foreach (Wall w in Walls.Values)
                {
                    if (w.CheckCollision(point, 51)) // walls are 50 by 50 so is a good saftey net around them to not spawn them inside of one
                    {
                        touched = true;
                        break;
                    }
                }

                if (!touched)
                {
                    randLoc = point;
                    break;
                }
            }

            return randLoc;
        }
    }
}
