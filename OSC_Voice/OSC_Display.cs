using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using System.Net.Sockets;
using OSC.Core;


namespace OSC.Display
{
    internal class OSC_DisplayTime
    {

        UdpClient udpClient;
        string IP = "127.0.0.1";
        string PATH = "/path";
        int DELAY = 1500;
        public OSC_DisplayTime(string ip, string path)
        {
            udpClient = new UdpClient();
            
            IP = ip;
            PATH = path;

            udpClient.Connect(IPAddress.Parse(IP), 9000);

        }
        public OSC_DisplayTime(string ip, string path, int delay)
        {
            udpClient = new UdpClient();
            IP = ip;
            PATH = path;
            if (delay > 1500)
            {
                DELAY = delay;
            }

            udpClient.Connect(IPAddress.Parse(IP), 9000);

        }

        public void StartDisplay()
        {
            TimeDisplay();
        }

        void TimeDisplay()
        {
            Byte[] sendBytes;
            try
            {

                OSC_Client Client = new OSC_Client();

                while (true)
                {
                    Console.WriteLine("Current time: " + DateTime.Now.ToString("hh:mm tt"));
                    sendBytes = Client.ConstructMessage(PATH, DateTime.Now.ToString("hh:mm tt"), true);
                    udpClient.Send(sendBytes, sendBytes.Length);
                    Task.Delay(DELAY).Wait();

                }

                udpClient.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    internal class OSC_DisplayTextFromFile
    {
        UdpClient udpClient;
        string IP = "127.0.0.1";
        string PATH = "/chatbox/input";
        int DELAY = 1500;
        bool LOOP = false;
        string FILE = "DisplayText.txt";
        public OSC_DisplayTextFromFile(string ip, string path)
        {
            udpClient = new UdpClient();
            IP = ip;
            PATH = path;

            udpClient.Connect(IPAddress.Parse(IP), 9000);
        }
        public OSC_DisplayTextFromFile(string ip, string path, int delay)
        {
            udpClient = new UdpClient();
            IP = ip;
            PATH = path;
            if (delay > 1500)
            {
                DELAY = delay;
            }

            udpClient.Connect(IPAddress.Parse(IP), 9000);
        }
        public OSC_DisplayTextFromFile(string ip, string path, int delay, bool loop)
        {
            udpClient = new UdpClient();
            IP = ip;
            PATH = path;
            if (delay > 1500)
            {
                DELAY = delay;
            }
            LOOP = loop;

            udpClient.Connect(IPAddress.Parse(IP), 9000);
        }

        public OSC_DisplayTextFromFile(string ip, string path, int delay, bool loop, string file)
        {
            udpClient = new UdpClient();
            IP = ip;
            PATH = path;
            if (delay > 1500)
            {
                DELAY = delay;
            }
            LOOP = loop;
            FILE = file;

            udpClient.Connect(IPAddress.Parse(IP), 9000);
        }

        public void StartDisplay()
        {
            TimeDisplay();
        }

        void TimeDisplay()
        {
            Byte[] sendBytes;
            try
            {
                OSC_Client Client = new OSC_Client();
                do
                {
                    foreach (string line in System.IO.File.ReadLines(FILE))
                    {
                        Console.WriteLine("Current Line: " + line);
                        sendBytes = Client.ConstructMessage(PATH, line, true);
                        udpClient.Send(sendBytes, sendBytes.Length);
                        Task.Delay(DELAY).Wait();
                    }
                } while (LOOP);

                udpClient.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
