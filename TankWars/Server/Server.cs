using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using NetworkUtil;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Made by Gabe Bautista and Blake Van Dyken
/// Started 12/5/21
/// 
/// This Class handles the server side of things for the Tank Wars game
/// </summary>
namespace TankWars
{
    class Server
    {
        private static TcpListener theServer;
        private static World theWorld;
        private static Stopwatch stopwatch;

        private static List<SocketState> allClients;

        // vars for xml file
        private static int MSPerFrame;
        private static int worldSize;
        private static int framesPerShot;
        private static int respawnRate;

        private static float engineStrength;
        private static int maxPowerUps;
        private static int maxPowerUpDelay;
        private static int startingHitPoints;
        private static int projectileSpeed;

        public static void Main(String[] args)
        {
            if (ReadSettings(out Stack<Wall> w))
            {
                StartServer(w);
            }
        }

        /// <summary>
        /// Helper method to Start the server in the main method
        /// </summary>
        private static void StartServer(Stack<Wall> w)
        {
            Console.WriteLine("Server Started");
            allClients = new List<SocketState>();
            stopwatch = new Stopwatch();
            theWorld = new World(worldSize, w, framesPerShot, respawnRate, engineStrength, maxPowerUps, maxPowerUpDelay, startingHitPoints, projectileSpeed);

            theServer = Networking.StartServer(new Action<SocketState>(AcceptNewClient), 11000);

            stopwatch.Start();
            while (true) // busy loop
            {
                while (stopwatch.ElapsedMilliseconds < MSPerFrame) { } // do nothing

                stopwatch.Restart();

                Update();
            }
        }

        /// <summary>
        /// Update the clients with the world by appending all commands to a stringbuilder and sending it
        /// </summary>
        private static void Update()
        {
            StringBuilder b = new StringBuilder();
            try
            {
                lock (theWorld) // add world stuff to a long command
                {

                    // update our world first 
                    theWorld.UpdateTheWorld();

                    // now make commands to update the client
                    foreach (Tank tank in theWorld.Tanks.Values)
                    {
                        b.Append(JsonConvert.SerializeObject(tank) + "\n");
                    }
                    foreach (Projectile proj in theWorld.Projectiles.Values)
                    {
                        b.Append(JsonConvert.SerializeObject(proj) + "\n");
                    }
                    foreach (Powerup pow in theWorld.Powerups.Values)
                    {
                        b.Append(JsonConvert.SerializeObject(pow) + "\n");
                    }
                    foreach (Beam beam in theWorld.Beams.Values)
                    {
                        b.Append(JsonConvert.SerializeObject(beam) + "\n");
                    }

                    // reset values and remove dead tanks, proj, etc.
                    theWorld.RemoveDeadStuff();
                }

                lock (allClients) // then update all the clients
                {
                    List<SocketState> clientsToRemove = new List<SocketState>();
                    foreach (SocketState state in allClients)
                    {
                        if (state.TheSocket.Connected) // the client is still connected
                        {
                            Networking.Send(state.TheSocket, b.ToString());
                        }
                        else // the client is not connected and we need to remove it
                        {
                            clientsToRemove.Add(state);
                        }
                    }

                    // remove all clients safley
                    lock (theWorld)
                    {
                        foreach (SocketState state in clientsToRemove)
                        {
                            if (theWorld.Tanks.ContainsKey((int)state.ID)) // see if our tank list has a tank we need to remove
                            {
                                theWorld.Tanks[(int)state.ID].DisconnectTank(0);
                            }

                            allClients.Remove(state);
                            Console.WriteLine("Client " + state.ID.ToString() + " has disconnected");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error updating clients: " + e);
            }
        }

        /// <summary>
        /// Start getting the data from the new client
        /// </summary>
        private static void AcceptNewClient(SocketState state)
        {
            if (!state.ErrorOccurred)
            {
                Console.WriteLine("New Client Connected with ID:" + (int)state.ID);
                state.OnNetworkAction = new Action<SocketState>(RecieveClientData);

                // get the data
                Networking.GetData(state);
            }
        }

        /// <summary>
        /// Send the client the player name and world size and all the walls
        /// </summary>
        private static void SendClientSetupInfo(Socket socket, int ID)
        {
            // first send player id and the world size
            string firstMsg = ID + "\n" + theWorld.size + "\n";
            Networking.Send(socket, firstMsg);

            string wallMsg = "";
            // send all the walls
            foreach (Wall w in theWorld.Walls.Values)
            {
                wallMsg += JsonConvert.SerializeObject(w) + "\n";
            }

            Networking.Send(socket, wallMsg); // this will show the wall I tested it and it works
        }

        /// <summary>
        /// Start asking for client data after it sets up the initial player and its tank
        /// </summary>
        /// <param name="state"></param>
        private static void RecieveClientData(SocketState state)
        {
            if (!state.ErrorOccurred)
            {
                Socket theSocket = state.TheSocket;
                int ID = (int)state.ID;
                string name = state.GetData().Trim().Replace("\n", "");
                state.OnNetworkAction = new Action<SocketState>(AskClientForData);

                lock (theWorld) // add a new tank to our world
                {
                    // add tank
                    Vector2D randLoc = theWorld.GetRandomTankLoc();
                    // create tank with new rand location
                    Tank newTank = new Tank(ID, name, randLoc, new Vector2D(0, 0), new Vector2D(0, 0), startingHitPoints);
                    newTank.JoinGame();
                    theWorld.Tanks.Add(ID, newTank);
                }

                // send Setup info
                SendClientSetupInfo(theSocket, ID);
                lock (allClients) // add the client to our list of clients
                {
                    allClients.Add(state);
                }

                Networking.GetData(state);
            }
        }

        /// <summary>
        /// Start asking the client for data over and over
        /// </summary>
        private static void AskClientForData(SocketState state)
        {
            if (!state.ErrorOccurred)
            {
                try
                {
                    int ID = (int)state.ID;
                    string[] commands = state.GetData().Trim().Split('\n'); // split all the commands sent to us

                    foreach (string s in commands)
                    {
                        if (s != "")
                        {
                            lock (theWorld)
                            {
                                if (!theWorld.Tanks.ContainsKey(ID)) // the player disconnected while we are doing this
                                    break;

                                // process client command
                                theWorld.UpdateTankWithCommand(theWorld.Tanks[ID], s);
                            }

                            // remove the data since we just processed it above
                            state.RemoveData(0, s.Length + 1); // +1 because we removed the \n char
                        }
                    }

                    Networking.GetData(state);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error getting client data: " + e);
                }
            }
        }

        /// <summary>
        /// REads the XML file and outs a stack of walls so we can use it for later things like sending it to the client
        /// </summary>
        private static bool ReadSettings(out Stack<Wall> walls)
        {
            try
            {
                walls = new Stack<Wall>();
                int i = 1;
                int x = 0;
                int y = 0;
                // create an Xml reader inside this block, and automatically dispose() it at the end
                using (XmlReader reader = XmlReader.Create(@"..\..\..\..\Resources\settings.xml"))
                {

                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {

                            switch (reader.Name)
                            {

                                case "UniverseSize":
                                    reader.Read();
                                    worldSize = int.Parse(reader.Value);
                                    break;
                                case "MSPerFrame":
                                    reader.Read();
                                    MSPerFrame = int.Parse(reader.Value);
                                    break;
                                case "RespawnRate":
                                    reader.Read();
                                    respawnRate = int.Parse(reader.Value);
                                    break;
                                case "FramesPerShot":
                                    reader.Read();
                                    framesPerShot = int.Parse(reader.Value);
                                    break;
                                case "Wall":
                                    Vector2D p1 = new Vector2D(0, 0);
                                    Vector2D p2 = new Vector2D(0, 0);
                                    reader.ReadToFollowing("x");
                                    reader.Read();
                                    x = int.Parse(reader.Value);
                                    reader.ReadToFollowing("y");
                                    reader.Read();
                                    y = int.Parse(reader.Value);
                                    p1 = new Vector2D(x, y);
                                    reader.ReadToFollowing("x");
                                    reader.Read();
                                    x = int.Parse(reader.Value);
                                    reader.ReadToFollowing("y");
                                    reader.Read();
                                    y = int.Parse(reader.Value);
                                    p2 = new Vector2D(x, y);
                                    Wall wall = new Wall(i++, p1, p2);
                                    walls.Push(wall);
                                    break;
                                case "EngineStrength":
                                    reader.Read();
                                    engineStrength = float.Parse(reader.Value);
                                    break;
                                case "MaxPowerUps":
                                    reader.Read();
                                    maxPowerUps = int.Parse(reader.Value);
                                    break;
                                case "MaxPowerUpDelay":
                                    reader.Read();
                                    maxPowerUpDelay = int.Parse(reader.Value);
                                    break;
                                case "StartingHitPoints":
                                    reader.Read();
                                    startingHitPoints = int.Parse(reader.Value);
                                    break;
                                case "ProjectileSpeed":
                                    reader.Read();
                                    projectileSpeed = int.Parse(reader.Value);
                                    break;
                            }
                        }
                    }
                }

                return true;
            }
            // catch any exception thrown and return the appropriate message
            catch (Exception e)
            {
                Console.WriteLine("Could not read XML: " + e);
                walls = new Stack<Wall>();
                return false;
            }
        }
    }
}