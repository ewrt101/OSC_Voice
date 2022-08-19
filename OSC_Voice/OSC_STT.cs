using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using STTClient;
using STTClient.Models;
using System.Net;
using System.Net.Sockets;
using OSC.Core;

namespace OSC.SpeechToText
{
    internal class OSC_STT
    {
        UdpClient udpClient;
        OSC_Client Client;
        string IP = "127.0.0.1";
        string PATH = "/path";
        string PATH2 = "/path2";
        int DEVICE = 0;


        string model = "";
        string scorer = "";
        //string audio = "arctic_a0024.wav";
        string audio = "test1.wav";

        STT? sttClient;

        WaveFileWriter? waveFile;
        WasapiCapture? waveSource;
        MMDeviceCollection? devices;
        public OSC_STT(string ip, string path, string path2, string _model, string _scorer, int _device)
        {
            
            IP = ip;
            PATH = path;
            PATH2 = path2;

            model = _model;
            scorer = _scorer;

            DEVICE = _device;  

            //set up OSC
            udpClient = new UdpClient();
            udpClient.Connect(IPAddress.Parse(IP), 9000);
            Client = new OSC_Client();

            //set up AI
            Stopwatch stopwatch = new Stopwatch();
            try
            {
                Console.WriteLine("Loading model...");
                stopwatch.Start();
                // sphinx-doc: csharp_ref_model_start
                sttClient = new STT(model);
                // sphinx-doc: csharp_ref_model_stop
                stopwatch.Stop();

                Console.WriteLine($"Model loaded - {stopwatch.Elapsed.Milliseconds} ms");
                stopwatch.Reset();
                if (scorer != null)
                {
                    Console.WriteLine("Loading scorer...");
                    sttClient.EnableExternalScorer(scorer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine();
            Console.WriteLine();


            //set up recording
            var deviceEnumerator = new MMDeviceEnumerator();
            devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            foreach (var device in devices)
            {
                Console.WriteLine(" " + device.DeviceFriendlyName);
            }
            Console.WriteLine();
            Console.WriteLine("Now listening on " + devices[_device].FriendlyName);
            waveSource = new WasapiCapture(devices[_device]);
        }

        public void Start()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Enter to exit");
            Console.ResetColor();
            RecordMic();
            //DetectWords();
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
                    waveFile = new WaveFileWriter(audio, waveSource.WaveFormat);
                    waveSource.StartRecording();


                    while (devices[DEVICE].AudioMeterInformation.MasterPeakValue > 0.02)
                    {
                        Thread.Sleep(1000);
                    }
                    waveSource.StopRecording();
                    waveFile.Dispose();

                    //decode audio to words
                    outWord = DetectWords();

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
            waveFile.Write(e.Buffer, 0, e.BytesRecorded);
        }

        public string DetectWords()
        {
            try
            {
                string audioFile = audio;
                var waveBuffer = new WaveBuffer(File.ReadAllBytes(audioFile));
                string speechResult;
                using (var waveInfo = new WaveFileReader(audioFile))
                {
                    
                    // sphinx-doc: csharp_ref_inference_start
                    speechResult = sttClient.SpeechToText(waveBuffer.ShortBuffer, Convert.ToUInt32(waveBuffer.MaxSize / 2));
                    // sphinx-doc: csharp_ref_inference_stop

                    Console.WriteLine("Recognized text: " + speechResult);
                    
                }
                waveBuffer.Clear();
                return speechResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

    }
}
