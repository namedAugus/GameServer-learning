using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class ChatClient : MonoBehaviour
{
    //定义套接字
    Socket socket;
    //UGUI
    public InputField inputField;
    public Text text;
    public Button connButton;
    public Button sendButton;

    //接收缓冲区
    byte[] readBuff = new byte[1024];
    string recvStr = "";

    private void Start()
    {
        inputField = GameObject.Find("Canvas/Root/InputField").GetComponent<InputField>();
        text = GameObject.Find("Canvas/Root/Text").GetComponent<Text>();

        connButton = GameObject.Find("Canvas/Root/ConnButton").GetComponent<Button>();
        sendButton = GameObject.Find("Canvas/Root/SendButton").GetComponent<Button>();
        connButton.onClick.AddListener(Connection);
        sendButton.onClick.AddListener(Send);
    }

    //点击连接按钮
    public void Connection()
    {
        //Socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Connect
        //socket.Connect("127.0.0.1", 8888);
        socket.BeginConnect("192.168.0.106", 8888, ConnectCallback, socket);
    }

    //Connect回调
    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Success!");

            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {

            Debug.Log("Socket Connect fail" + e.ToString());
        }
    }
    //Receive回调
    public void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            string temp = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
            recvStr = temp + "\n" + recvStr;
            socket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCallback, socket);
        }
        catch (SocketException e)
        {

            Debug.Log("Socket Receive fail" + e.ToString());
        }
    }

    //点击发送按钮
    public void Send()
    {
        //Send
        string sendStr = inputField.text;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        //socket.Send(sendBytes);
        socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        //Receive 使用异步，原来的这里注释
        //byte[] readBuff = new byte[1024];
        //int count = socket.Receive(readBuff);
        //string receiveStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
        //text.text = receiveStr;

        //Close
        //socket.Close();
    }

    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndSend(ar);
            Debug.Log("Socket Send Success!" + count);
        }
        catch (SocketException e)
        {

            Debug.Log("Socket Send fail" + e.ToString());
        }
    }

    private void Update()
    {
        //由于异步不在主线程中，unityUI需要在主线程中更新
        text.text = recvStr;
    }
}
