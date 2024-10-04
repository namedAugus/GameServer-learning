using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EchoServer
{
    internal class EchoServer
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            //Socket
            Socket listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            //Bind
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            listenfd.Bind(ipEp);
            
            //Listen
            listenfd.Listen(0);
            Console.WriteLine("服务器启动成功 -> Listening on port 8888");
            while (true)
            {
                //Accept
                Socket connfd = listenfd.Accept();
                Console.WriteLine(connfd.RemoteEndPoint.ToString());
                
                //时间
                string timeStr = "[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]";
                //Receive
                byte[] readBuff = new byte[1024];
                int count = connfd.Receive(readBuff);
                string readStr = System.Text.Encoding.UTF8.GetString(readBuff, 0, count); //服务端最好统一编码
                Console.WriteLine(timeStr + "服务器接收:" + readStr);
                
                byte[] sendBuff = Encoding.UTF8.GetBytes(timeStr + " " + readStr);
                connfd.Send(sendBuff);
            }
        }
    }
}