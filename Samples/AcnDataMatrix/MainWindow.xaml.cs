﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using LXProtocols.Acn;
using LXProtocols.Acn.Helpers;
using LXProtocols.Acn.Sockets;
using System.Net;
using LXProtocols.Acn.Packets.sAcn;
using LXProtocols.Acn.Rdm.Packets.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Timers;
using System.Threading;
using LXProtocols.Acn.IO;

namespace AcnDataMatrix
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SettingsViewModel settings;
        private AcnHandler adapter1Handler;
        private AcnHandler adapter2Handler;
        private MatrixWindow example;
        private System.Timers.Timer aTimer;
        private Thread gameThread;
        private Thread socket1Thread;
        private Thread socket2Thread;
        int universes = 0;
        StreamWriter debugInput = new StreamWriter("debug.txt");
        private AutoResetEvent windowReady = new AutoResetEvent(false);

        Dictionary<int, byte> sequenceChecker = new Dictionary<int, byte>();

        public SettingsViewModel Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStartApp_Click(object sender, RoutedEventArgs e)
        {
            windowReady.Reset();
            universes = 0;
            if (Settings.Net1Enabled)
            {
                universes += Settings.Net1UniverseCount;
            }

            if (Settings.Net2Enabled)
            {
                universes += Settings.Net2UniverseCount;
            }

            gameThread = new Thread(GameWindowThread);
            gameThread.IsBackground = true;
            gameThread.Start();
            windowReady.WaitOne();    

            if (Settings.Net1Enabled)
            {
                socket1Thread = new Thread(() => {
                    adapter1Handler = new AcnHandler(Settings.Net1, Settings.Net1StartUniverse, Settings.Net1UniverseCount);
                    foreach (StreamingAcnSocket socket in adapter1Handler.Sockets)
                    {
                        socket.NewPacket += socket_NewPacket;
                        socket.NewSynchronize += Socket_NewSynchronize;
                    }
                });
                socket1Thread.Start();
            }

            if (Settings.Net2Enabled)
            {
                socket2Thread = new Thread(() => {
                    adapter2Handler = new AcnHandler(Settings.Net2, Settings.Net2StartUniverse, Settings.Net2UniverseCount);
                    foreach (StreamingAcnSocket socket in adapter2Handler.Sockets)
                    {
                        socket.NewPacket += socket_NewPacket;
                        socket.NewSynchronize += Socket_NewSynchronize;
                    }
                });
                socket2Thread.Start();
                    
            }
                
            // Create a timer with a 500ms second interval.
            aTimer = new System.Timers.Timer(500);

            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            aTimer.Start();            
        }

        /// <summary>
        /// A list of the Sync IP address that we have seen. We wait for a sync packet from all of these before rendering
        /// </summary>
        List<string> SyncIps = new List<string>();

        /// <summary>
        /// A list of IP addresses of nodes that we have seeen sync packets from since the last render
        /// </summary>
        List<string> SeenIps = new List<string>();

        /// <summary>
        /// Called when a new sync packet is recieved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Socket_NewSynchronize(object sender, NewPacketEventArgs<StreamingAcnSynchronizationPacket> e)
        {
            string sourceIp = e.Source.Address.ToString();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!SyncIps.Contains(sourceIp))
                    SyncIps.Add(sourceIp);

                if (!SeenIps.Contains(sourceIp))
                    SeenIps.Add(sourceIp);

                if (SeenIps.Count == SyncIps.Count)
                {
                    example.Draw(TimeSpan.Zero);
                    SeenIps = new List<string>();
                }
            }));
        }

        /// <summary>
        /// Called when the reset sync button is clicked
        /// Clears out the list of IPs we're waiting for syncs from
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetSync_Click(object sender, RoutedEventArgs e)
        {
            SyncIps = new List<string>();
        }

        private void Socket_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void GameWindowThread()
        {
            example = new MatrixWindow(universes, true, "ACN Matrix");
            example.Unload += new EventHandler<EventArgs>(StopTimer);
            windowReady.Set();
            example.Run();
            
        }
        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //load settings
            if (File.Exists("settings.bin"))
            {
                Settings = loadSettings("settings.bin");
            }
            else
            {
                Settings = new SettingsViewModel();
            }

            grdNetwork.DataContext = Settings;
            lostPacket.DataContext = Settings;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //save settings
            saveSettings("settings.bin", Settings);

            if (gameThread != null)
            {
                gameThread.Join();

            }

            if (socket1Thread != null)
            {
                socket1Thread.Join();

            }

            if (socket2Thread != null)
            {
                socket2Thread.Join();
            }

            //debugInput.Flush();
            debugInput.Close();
        }

        public void saveSettings(string fileName, SettingsViewModel settings)
        {
            FileStream settingStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            BinaryFormatter bf = new BinaryFormatter();
            SerializeableNetworkSettings netSettings = new SerializeableNetworkSettings(Settings);
            bf.Serialize(settingStream, netSettings);
            settingStream.Close();
        }

        public SettingsViewModel loadSettings(string fileName)
        {
            try
            {
                FileStream settingStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                BinaryFormatter bf = new BinaryFormatter();
                SerializeableNetworkSettings settings = (SerializeableNetworkSettings)bf.Deserialize(settingStream);
                settingStream.Close();
                SettingsViewModel tempSettings = new SettingsViewModel();
                tempSettings.LoadSetting(settings);
                return tempSettings;
            }
            catch (System.Runtime.Serialization.SerializationException)
            {
                return new SettingsViewModel();
            }
        }


        void socket_NewPacket(object sender, NewPacketEventArgs<StreamingAcnDmxPacket> e)
        {
            StreamingAcnDmxPacket dmxPacket = e.Packet as StreamingAcnDmxPacket;
            if (example != null)
            {
                example.UpdateData(dmxPacket.Framing.Universe - 1, dmxPacket.Dmx.Data);

                if (dmxPacket.Framing.SyncPacketAddress == 0)
                    example.Draw(TimeSpan.FromMilliseconds(10));
            }            
        }       

        // Specify what you want to happen when the Elapsed event is  
        // raised. 
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (example != null)
            {
                if (example.UpdateTime1 != 0)
                {
                    Settings.FrameRate = 1000 / example.UpdateTime1;
                }
                else
                {
                    Settings.FrameRate = 1000 ;
                }
            }
        }
        //private event EventHandler StopTimer;

        private void StopTimer(object source, EventArgs e)
        {
            if (aTimer != null)
            {
                aTimer.Stop();
                aTimer.Close();
            }
        }

        private void ExceptionHandler()
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (example != null)
            {
                example.Width = Settings.Width;
                example.Height = Settings.Height;
            }
        }
    }
}
