using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TankWars
{
    public class GameController
    {

        // Controller events that the view can subscribe to
        public delegate void MessageHandler();
        public event MessageHandler MessagesArrived;

        public delegate void ConnectedHandler();
        public event ConnectedHandler Connected;

        public delegate void ErrorHandler(string err);
        public event ErrorHandler Error;

        public delegate void UpdateDrawingPanelHandler(World world, int myID);
        public event UpdateDrawingPanelHandler UpdateDrawingPanel;

        private bool movingPressed = false;
        private bool mousePressed = false;

        private int numberOfMessages = 0;

        private int TankID;
        private World theWorld;

        Stack<string> req;

        string movement;
        string firing = "none";
        Vector2D dir;

        /// <summary>
        /// State representing the connection with the server
        /// </summary>
        SocketState theServer = null;

        /// <summary>
        /// Begins the process of connecting to the server
        /// </summary>
        /// <param name="serverAddress"></param>
        public void ConnectToSever(string serverAddress)
        {
            Networking.ConnectToServer(OnConnect, serverAddress, 11000);
        }

        private void OnConnect(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                // inform the view
                Error("Error connecting to server");
                return;
            }

            theServer = state;

            // inform the view
            Connected();

            // Start an event loop to receive messages from the server
            state.OnNetworkAction = ReceiveMessage;
            Networking.GetData(state);
        }
        private void ReceiveMessage(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                // inform the view
                Error("Lost connection to server");
                return;
            }
            ProcessMessages(state);
            // Continue the event loop
            // state.OnNetworkAction has not been changed, 
            // so this same method (ReceiveMessage) 
            // will be invoked when more data arrives
            Networking.GetData(state);
        }


        /// <summary>
        /// Process any buffered messages separated by '\n'
        /// Then inform the view
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessages(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.

            List<string> newMessages = new List<string>();

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;
                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                // build a list of messages to send to the view
                newMessages.Add(p);

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }

            foreach (string m in newMessages)
            {
                if (numberOfMessages == 0) //this is the Tank id
                {
                    TankID = int.Parse(m);
                }
                else if (numberOfMessages == 1) //this is the world size
                {
                    //theWorld = new World(int.Parse(m));
                    //UpdateDrawingPanel(theWorld, TankID);
                }
                else // everthing else is JSON
                {
                    JObject obj = JObject.Parse(m);
                    JToken token = obj["wall"];

                    if (token != null)
                    {
                        Wall newWall = JsonConvert.DeserializeObject<Wall>(m);
                        if (!theWorld.Walls.ContainsKey(newWall.wall))
                            theWorld.Walls.Add(newWall.wall, newWall);
                    }

                    JToken token2 = obj["tank"];
                    if (token2 != null)
                    {
                        Tank rebuilt = JsonConvert.DeserializeObject<Tank>(m);
                        if (!theWorld.Tanks.ContainsKey(rebuilt.GetID()))
                            theWorld.Tanks.Add(rebuilt.GetID(), rebuilt);
                    }

                    JToken token3 = obj["proj"];
                    if (token3 != null)
                    {

                    }

                    JToken token4 = obj["beam"];
                    if (token4 != null)
                    {

                    }

                    JToken token5 = obj["power"];
                    if (token5 != null)
                    {
                       Powerup pow = JsonConvert.DeserializeObject<Powerup>(m);
                        if (!theWorld.Powerups.ContainsKey(pow.power))
                            theWorld.Powerups.Add(pow.power, pow);
                    }

                }
                //parse each message json and change the world
                numberOfMessages++;
            }

            // inform the view
            //this should update the DrawingPanel
            MessagesArrived();

        }

        /// <summary>
        /// Closes the connection with the server
        /// </summary>
        public void Close()
        {
            theServer?.TheSocket.Close();
        }

        /// <summary>
        /// Send a message to the server
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {

            if (theServer != null)
            {
                Networking.Send(theServer.TheSocket, message + "\n");
            }
            else
            {
                Console.WriteLine("server = null");
            }
        }

        /// <summary>
        /// Checks which inputs are currently held down
        /// this would send a message to the server
        /// </summary>
        public void ProcessInputs()
        {
            dir = new Vector2D(1f, 0f);
            if (movingPressed)
            {
                Console.WriteLine("{\"moving\":\"" + movement + "\",\"fire\":\"" + firing + "\",\"tdir\":{\"x\":" + dir.GetX() + ",\"y\":" + dir.GetY() + "}}");
                SendMessage("{\"moving\":\"" + movement + "\",\"fire\":\"" + firing + "\",\"tdir\":{\"x\":" + dir.GetX() + ",\"y\":" + dir.GetY() + "}}");
            }
            if (mousePressed)
                SendMessage("");
        }

        /// <summary>
        /// Example of handling movement request
        /// </summary>
        public void HandleMoveRequest(object o, KeyEventArgs key)
        {
            string item = "none";
            movingPressed = true;
            if (key.KeyCode == Keys.W)
                item = "up";
            else if (key.KeyCode == Keys.S)
                item = "down";
            else if (key.KeyCode == Keys.A)
                item = "left";
            else if (key.KeyCode == Keys.D)
                item = "right";
            if (key.KeyCode == Keys.Space)
                firing = "main";

            movement = item;
            movingPressed = true;
        }

        /// <summary>
        /// Example of handling movement request
        /// </summary>
        public void CancelMoveRequest(object o , KeyEventArgs key)
        {
            if(key.KeyCode == Keys.Space)
                firing = "none";
            if (key.KeyCode == Keys.W || key.KeyCode == Keys.S || key.KeyCode == Keys.A || key.KeyCode == Keys.D)
                movement = "none";
            movingPressed = false;
        }

        public void HandleMouseMoveRequest(object sender, FormClosedEventArgs e)
        {

        }
    }
}

