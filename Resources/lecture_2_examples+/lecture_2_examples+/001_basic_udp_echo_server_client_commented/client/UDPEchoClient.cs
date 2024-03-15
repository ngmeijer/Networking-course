using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace client
{
    class UDPEchoClient
    {
        public static void Main(string[] args)
        {
            //Note that we are not passing in a port number, 
            //this allows multiple clients to run, 
            //because every time a client runs it gets a random free port number assigned.
            UdpClient client = new UdpClient();

            while (true)
            {
                //Get, convert and send a message to the localhost at port 55555
                Console.WriteLine("Enter a message:");
                string outString = Console.ReadLine();
                Console.WriteLine("Sending:" + outString);
                byte[] outBytes = Encoding.UTF8.GetBytes(outString);
                client.Send(outBytes, outBytes.Length, "localhost", 55555);

                //Create an endPoint variable so that we can get information about the actual sender 
                //(which may be different every time a packet arrives)
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] inBytes = client.Receive(ref endPoint);
                string inString = Encoding.UTF8.GetString(inBytes);
                Console.WriteLine($"Received:{inString} ({inBytes.Length} bytes) from {endPoint}");
            }
        }
    }
}

//Things to try:
//Send to localhost without server running 
//Send to another host (that probably also doesn't have a server running on that port, eg google.com)
//Why does the first one crash, but not the second one? (Hint: research stack overflow)