﻿using System;
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
    internal class AsyncEchoServer
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
            Console.WriteLine(timeStr + "服务器启动成功 -> Listening on port 8888");
            
            //Accept 
            listenfd.BeginAccept(AcceptCallback, listenfd);
            //这里需要一行使程序等待的代码，否则程序到这就退出了
            Console.Read();
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
                if (count == 0) //小于等于0时表示socket连接断开了，但有一种特殊情况，此处先不处理(潜在bug)
                {
                    clientState.socket.Close();
                    clients.Remove(clientfd);
                    Console.WriteLine("Socket Closed");
                    return;
                }
                
                string recvStr = Encoding.UTF8.GetString(clientState.readBuffer, 0, count);
                Console.WriteLine(timeStr + "[" + clientState.socket.RemoteEndPoint.ToString() + "] " + recvStr);
                byte[] sendBytes = Encoding.UTF8.GetBytes("echo"+recvStr);
                clientfd.Send(sendBytes); //这里可以不用异步，减少代码量
                clientfd.BeginReceive(clientState.readBuffer, 0, 1024, 0, ReceiveCallback, clientState);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}