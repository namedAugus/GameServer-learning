using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class Echo : MonoBehaviour
{
    //定义套接字
    Socket socket;
    //UGUI
    public InputField inputField;
    public Text text;
    public Button connButton;
    public Button sendButton;

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
        socket.Connect("127.0.0.1", 8888);
    }

    //点击发送按钮
    public void Send()
    {
        //Send
        string sendStr = inputField.text;
        byte[] sendBytes = System.Text.Encoding.Default.GetBytes(sendStr);
        socket.Send(sendBytes);

        //Receive
        byte[] readBuff = new byte[1024];
        int count = socket.Receive(readBuff);
        string receiveStr = System.Text.Encoding.Default.GetString(readBuff, 0, count);
        text.text = receiveStr;

        //Close
        socket.Close();
    }
}
