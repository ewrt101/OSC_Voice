using System.Net;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using OSC.Core;
using OSC.Display;
using OSC.SpeechToText;
using SpeechToText.AssemblyAI;
using NAudio.CoreAudioApi;

class Program
{
    static void Main(string[] args)
    {
        string input = "";

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Select module to run");
            Console.ResetColor();

            Console.WriteLine("1)   Display time");
            Console.WriteLine("2)   Read from text file");
            Console.WriteLine("3)   Speech To Text (Local)");
            Console.WriteLine("4)   Speech To Text (AssemblyAI Realtime)");
            Console.WriteLine("5)   Speech To Text (AssemblyAI Chunk)");
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("T)   Test");
            Console.ResetColor();
            Console.WriteLine("");


            //select module
            input = Console.ReadLine().ToLower();
            Console.Clear();
            if (input == "time" || input == "1")
            {
                TimeDisplay();
            }
            if (input == "file" || input == "2")
            {
                ReadFileDisplay();
            }
            if (input == "stt" || input == "3")
            {
                STT();
            }
            if (input == "assemblyai" || input == "4")
            {
                AssemblyAI();
            }

            if (input == "AssemblyAIChunk" || input == "5")
            {
                AssemblyAIChunk();
            }

            //test
            if (input == "test" || input == "t")
            {
                Testing();
            }
        }
    }

    
    static int PickMic()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Pick a Mic #");
        Console.ResetColor();

        var deviceEnumerator = new MMDeviceEnumerator();
        var devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
        for (var i = 0; i < devices.Count; i++)
        //foreach (var device in devices)
        {
            Console.WriteLine((i + 1).ToString() + ":   " + devices[i].DeviceFriendlyName);
        }
        var input2 = Console.ReadLine();
        int number = Int32.Parse(input2);
        Console.Clear();
        return number-1;
    }


    //
    // DIFFRENT MODE START FUNCTIONS
    //
    static void Testing()
    {
        UdpClient udpClient = new UdpClient();
        string IP = "127.0.0.1";
        try
        {
            udpClient.Connect(IPAddress.Parse(IP), 9000);

            OSC_Client Client = new OSC_Client();

            Byte[] sendBytes = Client.ConstructMessage("/Tester/ewrtInt", 10);
            Byte[] sendBytes2 = Client.ConstructMessage("/Tester/ewrtFloat", 0.2f);
            Byte[] sendBytes3 = Client.ConstructMessage("/chatbox/input", "tester", false);
            //Byte[] sendBytes3 = Client.ConstructMessage("/chatbox/input", "testerrrr");
            udpClient.Send(sendBytes, sendBytes.Length);
            udpClient.Send(sendBytes2, sendBytes2.Length);
            udpClient.Send(sendBytes3, sendBytes3.Length);

            udpClient.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    static void TimeDisplay()
    {
        OSC_DisplayTime displayTime = new OSC_DisplayTime("127.0.0.1", "/chatbox/input");
        displayTime.StartDisplay();
    }

    static void ReadFileDisplay()
    {
        OSC_DisplayTextFromFile fileTime = new OSC_DisplayTextFromFile("127.0.0.1", "/chatbox/input", 5000, false, "DisplayText.txt");
        fileTime.StartDisplay();
    }

    static void STT()
    {
        OSC_STT stt = new OSC_STT("127.0.0.1", "/chatbox/input", "/chatbox/typing", "model.tflite", "vocabulary.scorer", PickMic());
        stt.Start();
    }

    static void AssemblyAI()
    {
        string[] lines = System.IO.File.ReadAllLines("AssemblyAI.key");
        AssemblyAILive realtime = new AssemblyAILive("127.0.0.1", "/chatbox/input", "/chatbox/typing", PickMic());
        realtime.Start(lines[0], 3600);
    }

    static void AssemblyAIChunk()
    {
        string[] lines = System.IO.File.ReadAllLines("AssemblyAI.key");
        AssemblyAIChunk Chunk = new AssemblyAIChunk("127.0.0.1", "/chatbox/input", "/chatbox/typing", PickMic());
        Chunk.Start(lines[0]);
    }

}