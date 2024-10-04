using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EchoServer
{
    class ClientState
    {
        public Socket socket;
        public byte[] readBuffer = new byte[1024];
    }
    internal class CharEchoServer
    {
        static Socket listenfd;
        static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            //Socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Bind
            IPAddress ipAdr = IPAddress.Parse("192.168.0.106");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            listenfd.Bind(ipEp);
            
            //Listen
            listenfd.Listen(0);//0代表不限制连接的客户端数
            //时间
            string timeStr = "[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]";
            Console.WriteLine(timeStr + "服务器启动成功 -> Listening on port 8888");
            
            //Accept 
            listenfd.BeginAccept(AcceptCallback, listenfd);
            //这里需要一行使程序等待的代码，否则程序到这就退出了
            Console.ReadLine();
        }
        
        //Accept回调
        public static void AcceptCallback(IAsyncResult ar)
        {
            string timeStr = "[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]";
            try
            {
                
                Console.WriteLine(timeStr + "服务器Accept...");
                Socket listenfd = (Socket)ar.AsyncState;
                Socket clientfd = listenfd.EndAccept(ar);
                
                //client列表
                ClientState clientState = new ClientState();
                clientState.socket = clientfd;
                clients.Add(clientfd, clientState);
                //接收数据
                clientfd.BeginReceive(clientState.readBuffer, 0, 1024, 0, ReceiveCallback, clientState);
                //继续Accept
                listenfd.BeginAccept(AcceptCallback, listenfd);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }

        //Receive回调
        public static void ReceiveCallback(IAsyncResult ar)
        {
            string timeStr = "[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]";
            try
            {
                ClientState clientState = (ClientState)ar.AsyncState;
                Socket clientfd = clientState.socket;
                int count = clientState.socket.EndReceive(ar);
                //客户端关闭
                if (count == 0)
                {
                    clientState.socket.Close();
                    clients.Remove(clientfd);
                    Console.WriteLine("Socket Closed");
                    return;
                }
                
                string recvStr = Encoding.UTF8.GetString(clientState.readBuffer, 0, count);
                Console.WriteLine(timeStr + "[" + clientState.socket.RemoteEndPoint.ToString() + "] " + recvStr);
                byte[] sendBytes = Encoding.UTF8.GetBytes("echo"+recvStr);
                // clientfd.Send(sendBytes); //这一行代码改成遍历即可成为聊天室
                foreach (KeyValuePair<Socket,ClientState> client in clients)
                {
                    client.Value.socket.Send(sendBytes);
                }
                clientfd.BeginReceive(clientState.readBuffer, 0, 1024, 0, ReceiveCallback, clientState);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}