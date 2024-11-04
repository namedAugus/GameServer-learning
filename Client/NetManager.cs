using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public static class NetManager
{
    //定义套接字
    public static Socket socket;
    //接收换冲区
    static byte[] readBuff = new byte[1024];
    //委托类型
    public delegate void MsgListener(string str);
    //监听列表
    private static Dictionary<string, MsgListener> listeners = new Dictionary<string, MsgListener>();
    //消息列表
    static List<String> msgList = new List<string>();

    //添加监听
    public static void AddListener(string msgName, MsgListener listener)
    {
        listeners[msgName] = listener;
    }

    //获取描述
    public static string GetDesc()
    {
        if (socket == null) return "";
        if (!socket.Connected) return "";
        return socket.LocalEndPoint.ToString();
    }

    //连接
    public static void Connect(string ip, int port)
    {
        //Sokcet
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Connect(用同步的方式简化代码)
        socket.Connect(ip, port);
        //BeginReceive
        socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
    }

    //Receive回调
    public static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            string recvStr = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
            msgList.Add(recvStr);
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {
            //Debug.Log("Socket Receive fail" + e.ToString());
            Console.WriteLine("Socket Receive fail" + e.ToString());
        }
    }

    //Send
    public static int Send(string sendStr)
    {
        if (socket == null) return 0;
        if (!socket.Connected) return 0;

        byte[] sendBytes = System.Text.Encoding.UTF8.GetBytes(sendStr);
        return socket.Send(sendBytes);
    }

    //Update
    public static void Update()
    {

        if (msgList.Count <= 0) return;

        string msgStr = msgList[0];
        msgList.RemoveAt(0);
        string[] split = msgStr.Split('|');
        string msgName = split[0];
        string msgArgs = split[1];

        //监听回调
        if (listeners.ContainsKey(msgName))
        {
            listeners[msgName](msgArgs);
        }

    }
}
