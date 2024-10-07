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
    internal class SelectEchoServer
    {
        static Socket listenfd;
        static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            
            //Socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Bind
            IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
            listenfd.Bind(ipEp);
            
            //Listen
            listenfd.Listen(0);//0代表不限制连接的客户端数
            //时间
            string timeStr = "[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]";
            Console.WriteLine(timeStr + "服务器启动成功 Start Listening on port 8888");
            
            //CheckRead
            List<Socket> checkRead = new List<Socket>();
            //主循环
            while (true)
            {
                checkRead.Clear();
                //填充checkRead列表
                checkRead.Add(listenfd);
                foreach (ClientState s in clients.Values)
                {
                    checkRead.Add(s.socket);
                }
                //select
                Socket.Select(checkRead, null, null, 1000);
                //检查可读对象
                foreach (Socket s in checkRead)
                {
                    if (s == listenfd)
                    {
                        ReadListenfd(s);
                    }
                    else
                    {
                        ReadClientfd(s);
                    }
                }
            }
        }
        
        
        
        //读取Listenfd
        public static void ReadListenfd(Socket listenfd)
        {
            Console.WriteLine("Accept");
            Socket clientfd = listenfd.Accept();
            ClientState clientState = new ClientState(); 
            clientState.socket = clientfd;
            clients.Add(clientfd, clientState);
        }
        
        //读取clientfd
        public static bool ReadClientfd(Socket clientfd)
        {
            ClientState clientState = clients[clientfd];
            //接收
            int count = 0;
            try
            {
                count = clientfd.Receive(clientState.readBuffer);
                
            }
            catch (SocketException e)
            {
                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine(e);
                return false;
            }
            
            //客户端关闭
            if (count == 0)
            {
                clientfd.Close();
                clients.Remove(clientfd);
                Console.WriteLine("Socket Closed");
                return false;
            }
            
            //广播
            string recvStr = Encoding.UTF8.GetString(clientState.readBuffer, 0, count);
            Console.WriteLine("Receive " + recvStr);
            string sendStr = clientfd.RemoteEndPoint.ToString() + ":" + recvStr;
            byte[] sendBytes = Encoding.UTF8.GetBytes(sendStr);
            foreach (ClientState cs in clients.Values)
            {
                cs.socket.Send(sendBytes);
            }
            return true;
        }
    }
}