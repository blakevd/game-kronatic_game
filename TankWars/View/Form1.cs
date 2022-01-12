using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TankWars
{
    public partial class Form1 : Form
    {
        private GameController theController;
        private DrawingPanel drawingPanel;

        Label serverLabel, nameLabel;
        TextBox serverText, nameText;
        Button startButton;

        private const int viewSize = 900;
        private const int menuSize = 40;

        public Form1(GameController ctl)
        {
            InitializeComponent();
            theController = ctl;

            // set the window size
            ClientSize = new Size(viewSize, viewSize + menuSize);

            // register handlers for the controller's events
            theController.MessagesArrived += OnFrame;
            theController.Error += ShowError;
            theController.Connected += HandleConnected;
            theController.UpdateDrawingPanel += UpdateDrawingPanel;

            // Place and add the server label
            serverLabel = new Label();
            serverLabel.Text = "Server:";
            serverLabel.Location = new Point(5, 10);
            serverLabel.Size = new Size(50, 15);
            this.Controls.Add(serverLabel);

            // Place and add the server textbox
            serverText = new TextBox();
            serverText.Text = "localhost";
            serverText.Location = new Point(55, 7);
            serverText.Size = new Size(100, 15);
            this.Controls.Add(serverText);

            // Place and add the name label
            nameLabel = new Label();
            nameLabel.Text = "Name:";
            nameLabel.Location = new Point(155, 10);
            nameLabel.Size = new Size(50, 15);
            this.Controls.Add(nameLabel);

            // Place and add the name textbox
            nameText = new TextBox();
            nameText.Text = "player";
            nameText.Location = new Point(205, 7);
            nameText.Size = new Size(100, 15);
            this.Controls.Add(nameText);

            // Place and add the button
            startButton = new Button();
            startButton.Location = new Point(315, 7);
            startButton.Size = new Size(70, 20);
            startButton.Text = "Connect";
            startButton.Click += StartClick;
            this.Controls.Add(startButton);

            // create the drawing panel
            drawingPanel = new DrawingPanel(null, -1);
            drawingPanel.Location = new Point(0, menuSize);
            drawingPanel.Size = new Size(viewSize, viewSize);
            this.Controls.Add(drawingPanel);

            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;
            //drawingPanel.MouseMove += HandleMouseMove;
            //drawingPanel.MouseDown += HandleMouseDown;
            // drawingPanel.MouseUp += HandleMouseUp;
        }


        /// <summary>
        /// Connect to server
        /// </summary>
        private void StartClick(object sender, EventArgs e)
        {
            //connect button pressed
            theController.ConnectToSever(serverText.Text);
            // Disable the form controls
            startButton.Enabled = false;
            nameText.Enabled = false;
            // Enable the global form to capture key presses
            drawingPanel.Focus();
            KeyPreview = true;

        }

        private void HandleMouseMove(object sender, FormClosedEventArgs e)
        {
            theController.HandleMouseMoveRequest( sender,  e);
        }
        private void HandleConnected()
        {
            //connect to server
            Console.WriteLine("Conneted to server");
            //send name first
            theController.SendMessage(nameText.Text);
            //then server sends first player id then and world size as two strings
        }

        private void UpdateDrawingPanel(World world, int p)
        {
            drawingPanel.addWorldAndTank(world, p);
        }

        /// <summary>
        /// Handler for the controller's Error event
        /// </summary>
        /// <param name="err"></param>
        private void ShowError(string err)
        {
            // Show the error
            MessageBox.Show(err);
            // Then re-enable the controlls so the user can reconnect
            this.Invoke(new MethodInvoker(
              () =>
              {
                  startButton.Enabled = true;
                  serverText.Enabled = true;
                  nameText.Enabled = true;
              }));
        }

        /// <summary>
        /// Handler for the controllers MessagesArrived event
        /// </summary>
        /// <param name="newMessages"></param>
        private void OnFrame()
        {
            theController.ProcessInputs();

            // redraw as soon as it can
            MethodInvoker invoker = new MethodInvoker(() => Invalidate(true));
            Invoke(invoker);
            
        }

        /// <summary>
        /// Handle the form closing by shutting down the socket cleanly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExit(object sender, FormClosedEventArgs e)
        {
            theController.Close();
        }

        /// <summary>
        /// Key down handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Escape)
                Application.Exit();
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.S || e.KeyCode == Keys.A || e.KeyCode == Keys.D || e.KeyCode == Keys.Space)
                theController.HandleMoveRequest(sender, e);

            // Prevent other key handlers from running
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                theController.CancelMoveRequest(sender, e);
        }
    }
}
