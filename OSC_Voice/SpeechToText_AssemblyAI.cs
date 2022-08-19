using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Websocket.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Net.Http;

using NAudio.Wave;
using NAudio.CoreAudioApi;

using System.Net;
using System.Net.Sockets;
using OSC.Core;

namespace SpeechToText.AssemblyAI
{
    internal class AssemblyAILive
    {

        UdpClient udpClient;
        OSC_Client Client;
        string IP = "127.0.0.1";
        string PATH = "/path";
        string PATH2 = "/path2"; // not used
        int DEVICE = 0;


        string TOKEN_URI = "https://api.assemblyai.com/v2/realtime/token";
        string uri = "wss://api.assemblyai.com/v2/realtime/ws?sample_rate=16000&token=";

        WebsocketClient client;
        Uri url;
        MemoryStream memStream = new MemoryStream(0);

        long milliseconds;

        WasapiCapture? waveSource;
        MMDeviceCollection? devices;
        public AssemblyAILive(string ip, string path, string path2, int _device)
        {
            IP = ip;
            PATH = path;
            PATH2 = path2;

            DEVICE = _device;


            //set up OSC
            udpClient = new UdpClient();
            udpClient.Connect(IPAddress.Parse(IP), 9000);
            Client = new OSC_Client();
        }

        public void Start(string key, int TokenLife)
        {
            //create Token & URL
            string token = GetTokenAsync(key, TokenLife).GetAwaiter().GetResult();
            Console.WriteLine("Token: " + token);
            url = new Uri(uri + token);
            Console.WriteLine(url);

            //connect to server
            ConnectToServer();
            RecordMic();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Enter to exit");
            Console.ResetColor();
            Console.ReadKey();

            //clean up
            TermateSession();
            waveSource.StopRecording();
            //just wait a few seconds to make sure it closes
            Thread.Sleep(2000);
            Console.WriteLine();
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine();
            Console.WriteLine();
        }
        async Task<string> GetTokenAsync(string key, int TokenLife)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add(
                  "Authorization",
                  key
                );


                var json = new
                {
                    expires_in = TokenLife
                };

                StringContent payload = new StringContent(
                  JsonConvert.SerializeObject(json),
                  Encoding.UTF8,
                  "application/json"
                );
                HttpResponseMessage response = await httpClient.PostAsync(TOKEN_URI,payload);

                response.EnsureSuccessStatusCode();
                var responseJson = response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(await responseJson);
                string name = (string)obj["token"];
                return name;
            }
        }

        void ConnectToServer()
        {
            client = new WebsocketClient(url);
            client.IsReconnectionEnabled = false;
            client.MessageReceived.Subscribe(msg =>
            {
                DecodeMessage(JObject.Parse(msg.ToString()));
            });

            client.DisconnectionHappened.Subscribe(msg =>
            {
                Console.WriteLine("Disconnect");
                
            });
            client.Start();  
        }

        void DecodeMessage(JObject msg)
        {
            if (msg["message_type"] == null){
                Console.WriteLine(msg);
            }
            //Console.WriteLine("Message received: " + msg.ToString());
            if (msg["message_type"].ToString() == "SessionBegins")
            {
                Console.WriteLine("SessionBegins");
            }
            else if (msg["message_type"].ToString() == "SessionTerminated")
            {
                Console.WriteLine("SessionTerminated"); 
                CloseClient();
            }
            else if (msg["message_type"].ToString() == "FinalTranscript")
            {
                if (msg["text"].ToString() != "")
                {
                    Console.WriteLine("Spoken Words: " + msg["text"].ToString());
                    SendText(msg["text"].ToString());
                } 
            }
            else if (msg["message_type"].ToString() == "PartialTranscript")
            {
                //DO nothing kek
            }
            else
            {
                Console.WriteLine("unknown message_type: " + msg["message_type"].ToString());
                Console.WriteLine(msg);
            }
        }

        void SendText(string outWord)
        {
            Byte[] sendBytes;
            sendBytes = Client.ConstructMessage(PATH, outWord, true);
            udpClient.Send(sendBytes, sendBytes.Length);
        }

        void TermateSession()
        {
            JObject obj = new JObject();
            obj["terminate_session"] = true;

            client.Send(obj.ToString());
        }

        void CloseClient()
        {
            Console.WriteLine("Closing Client");
            client.Dispose();
        }

        

        void RecordMic()
        {
            //set up recording
            var deviceEnumerator = new MMDeviceEnumerator();
            devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            foreach (var device in devices)
            {
                Console.WriteLine(" " + device.DeviceFriendlyName);
            }
            Console.WriteLine();
            Console.WriteLine("Now listening on " + devices[DEVICE].FriendlyName);
            waveSource = new WasapiCapture(devices[DEVICE]);
            waveSource.WaveFormat = new WaveFormat(16000, 1);
            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            waveSource.StartRecording();
            milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            memStream.Write(e.Buffer, 0, e.BytesRecorded);
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() > milliseconds + 250)
            {
                milliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                byte[] buffer = memStream.ToArray();
                memStream.SetLength(0);

                JObject obj = new JObject();
                obj["audio_data"] = Convert.ToBase64String(buffer, 0, buffer.Length);

                client.Send(obj.ToString());
            }
        }
    }

    internal class AssemblyAIChunk
    {

        UdpClient udpClient;
        OSC_Client Client;
        string IP = "127.0.0.1";
        string PATH = "/path";
        string PATH2 = "/path2"; // not used
        int DEVICE = 0;

        string KEY = "";


        string STREAM_URI = "https://api.assemblyai.com/v2/stream";

        WebsocketClient client;
        Uri url;
        MemoryStream memStream = new MemoryStream(0);

        WasapiCapture? waveSource;
        MMDeviceCollection? devices;

        public AssemblyAIChunk(string ip, string path, string path2, int _device)
        {
            IP = ip;
            PATH = path;
            PATH2 = path2;

            DEVICE = _device;


            //set up OSC
            udpClient = new UdpClient();
            udpClient.Connect(IPAddress.Parse(IP), 9000);
            Client = new OSC_Client();

            

        }

        public void Start(string key)
        {
            KEY = key;
            //set up recording
            var deviceEnumerator = new MMDeviceEnumerator();
            devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            Console.WriteLine();
            Console.WriteLine("Now listening on " + devices[DEVICE].FriendlyName);
            waveSource = new WasapiCapture(devices[DEVICE]);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Enter to exit");
            Console.ResetColor();

            RecordMic();
        }

        async Task<string> SendPacket(string key, string data, bool format, bool punct)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add(
                  "Authorization",
                  key
                );


                var json = new
                {
                    audio_data = data,
                    format_text = format,
                    punctuate = punct
                };

                StringContent payload = new StringContent(
                  JsonConvert.SerializeObject(json),
                  Encoding.UTF8,
                  "application/json"
                );
                HttpResponseMessage response = await httpClient.PostAsync(STREAM_URI, payload);

                if (response.IsSuccessStatusCode == false)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("You must speek for less than 15 seconds");
                    Console.ResetColor();
                    return "";
                }

                var responseJson = response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(await responseJson);
                string words = (string)obj["text"];
                return words;
            }
        }

        public void RecordMic()
        {
            Byte[] sendBytes;
            string outWord;
            bool triggered = false;
            ConsoleKeyInfo cki;
            waveSource.WaveFormat = new WaveFormat(16000, 1);
            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);
            while (true)
            {
                if (devices[DEVICE].AudioMeterInformation.MasterPeakValue > 0.05)
                {
                    //set typing display
                    sendBytes = Client.ConstructMessage(PATH2, true);
                    udpClient.Send(sendBytes, sendBytes.Length);

                    //record audio
                    waveSource.StartRecording();

                    while (devices[DEVICE].AudioMeterInformation.MasterPeakValue > 0.02)
                    {
                        Thread.Sleep(100);
                    }
                    waveSource.StopRecording();

                    byte[] buffer = memStream.ToArray();
                    memStream.SetLength(0);

                    //decode audio to words
                    outWord = SendPacket(KEY, Convert.ToBase64String(buffer, 0, buffer.Length), true, true).GetAwaiter().GetResult();

                    Console.WriteLine("Text: " + outWord);

                    //check if quit
                    if (outWord == "leave program")
                    {
                        //unset typing display
                        sendBytes = Client.ConstructMessage(PATH2, false);
                        udpClient.Send(sendBytes, sendBytes.Length);
                        return;
                    }

                    //send msg
                    sendBytes = Client.ConstructMessage(PATH, outWord, true);
                    udpClient.Send(sendBytes, sendBytes.Length);
                    //unset typing display
                    sendBytes = Client.ConstructMessage(PATH2, false);
                    udpClient.Send(sendBytes, sendBytes.Length);
                }
                
                
                if (Console.KeyAvailable == true)
                {
                    cki = Console.ReadKey(true);
                    if (cki.Key == ConsoleKey.Enter)
                    {
                        return;
                    }
                } 
            }
        }

        void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            memStream.Write(e.Buffer, 0, e.BytesRecorded);
        }
    }
}
