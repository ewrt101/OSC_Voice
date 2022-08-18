using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSC.Core
{
    //https://ccrma.stanford.edu/groups/osc/spec-1_0.html
    //https://ccrma.stanford.edu/groups/osc/spec-1_0-examples.html#bundledispatchorder
    internal class OSC_Client
    {
        public OSC_Client()
        {

        }


        public byte[] ConstructMessage(string path, int number)
        {
            //Address 
            Byte[] sendBytes = Encoding.ASCII.GetBytes(path);
            sendBytes = AddByteToArray(sendBytes, 0x00); // need a null byte space
            sendBytes = AddByteArrayPadding(sendBytes);
            //Tag
            sendBytes = AddByteToArray(sendBytes, 0x2c);
            sendBytes = AddByteToArray(sendBytes, Convert.ToByte('i'));
            sendBytes = AddByteArrayPadding(sendBytes);
            //number
            sendBytes = AddByteArrayToArray(sendBytes, BitConverter.GetBytes(number), BitConverter.IsLittleEndian);

            return sendBytes;
        }

        public byte[] ConstructMessage(string path, float number)
        {
            //Address 
            Byte[] sendBytes = Encoding.ASCII.GetBytes(path);
            sendBytes = AddByteToArray(sendBytes, 0x00); // need a null byte space
            sendBytes = AddByteArrayPadding(sendBytes);
            //Tag
            sendBytes = AddByteToArray(sendBytes, 0x2c);
            sendBytes = AddByteToArray(sendBytes, Convert.ToByte('f'));
            sendBytes = AddByteArrayPadding(sendBytes);
            //number
            sendBytes = AddByteArrayToArray(sendBytes, BitConverter.GetBytes(number), BitConverter.IsLittleEndian);

            return sendBytes;
        }

        public byte[] ConstructMessage(string path, string Msg)
        {
            //Address 
            Byte[] sendBytes = Encoding.ASCII.GetBytes(path);
            sendBytes = AddByteToArray(sendBytes, 0x00); // need a null byte space
            sendBytes = AddByteArrayPadding(sendBytes);
            //Tag
            sendBytes = AddByteToArray(sendBytes, 0x2c);
            sendBytes = AddByteToArray(sendBytes, Convert.ToByte('s'));
            //sendBytes = AddByteToArray(sendBytes, Convert.ToByte('i'));
            sendBytes = AddByteArrayPadding(sendBytes);
            //String
            sendBytes = AddByteArrayToArray(sendBytes, Encoding.ASCII.GetBytes(Msg), false);
            sendBytes = AddByteToArray(sendBytes, 0x00);
            sendBytes = AddByteArrayPadding(sendBytes);

            return sendBytes;
        }

        public byte[] ConstructMessage(string path, bool is_true)
        {
            //Address 
            Byte[] sendBytes = Encoding.ASCII.GetBytes(path);
            sendBytes = AddByteToArray(sendBytes, 0x00); // need a null byte space
            sendBytes = AddByteArrayPadding(sendBytes);
            //Tag
            sendBytes = AddByteToArray(sendBytes, 0x2c);
            if (is_true)
            {
                sendBytes = AddByteToArray(sendBytes, Convert.ToByte('T')); //set bool
            }
            else
            {
                sendBytes = AddByteToArray(sendBytes, Convert.ToByte('F')); //set bool
            }

            sendBytes = AddByteArrayPadding(sendBytes);
            return sendBytes;
        }

        public byte[] ConstructMessage(string path, string Msg, bool is_forceSend)
        {
            //Address 
            Byte[] sendBytes = Encoding.ASCII.GetBytes(path);
            sendBytes = AddByteToArray(sendBytes, 0x00); // need a null byte space
            sendBytes = AddByteArrayPadding(sendBytes);
            //Tag
            sendBytes = AddByteToArray(sendBytes, 0x2c);
            sendBytes = AddByteToArray(sendBytes, Convert.ToByte('s'));
            if (is_forceSend)
            {
                sendBytes = AddByteToArray(sendBytes, Convert.ToByte('T')); //set bool
            }
            else
            {
                sendBytes = AddByteToArray(sendBytes, Convert.ToByte('F')); //set bool
            }
            
            sendBytes = AddByteArrayPadding(sendBytes);
            //String
            sendBytes = AddByteArrayToArray(sendBytes, Encoding.ASCII.GetBytes(Msg), false);
            sendBytes = AddByteToArray(sendBytes, 0x00);
            sendBytes = AddByteArrayPadding(sendBytes);
            return sendBytes;
        }

        //pads out array
        static public byte[] AddByteArrayPadding(byte[] bArray, int multiple = 4)
        {
            while (bArray.Length % multiple != 0)
            {
                bArray = AddByteToArray(bArray, 0x00);
            }
            return bArray;
        }

        //used for adding a single byte to an array
        static public byte[] AddByteToArray(byte[] bArray, byte newByte)
        {
            byte[] newArray = new byte[bArray.Length + 1];
            bArray.CopyTo(newArray, 0);
            newArray[bArray.Length] = newByte;
            return newArray;
        }

        // used for joining two byte arrays
        static public byte[] AddByteArrayToArray(byte[] bArray, byte[] newByte, bool flip = false)
        {
            //if we are told to flip the array
            if (flip)
            {
                Array.Reverse(newByte);
            }
            //construct new array and copy in arrays
            byte[] newArray = new byte[bArray.Length + newByte.Length];
            bArray.CopyTo(newArray, 0);
            newByte.CopyTo(newArray, bArray.Length);

            return newArray;
        }

    }
}
