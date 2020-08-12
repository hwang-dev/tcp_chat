using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    public GameObject chatContainer;
    public GameObject msgPrefab;

    private bool _socketReady;
    private TcpClient _socket;
    private NetworkStream _stream;
    private StreamWriter _writter;
    private StreamReader _reader;


    private void Update()
    {
        if (_socketReady)
        {
            if (_stream.DataAvailable)
            {
                string data = _reader.ReadLine();
                if (data != null)
                    OnIncomingData(data);
            }
        }
    }

    private void OnIncomingData(string data)
    {
        Debug.Log($"----> server: {data}");
        var go = Instantiate(msgPrefab, chatContainer.transform) as GameObject;
        if (go != null && !string.IsNullOrEmpty(data))
            go.GetComponentInChildren<Text>().text = data;
    }

    public void OnConnectedToServer()
    {
        if (_socketReady)
            return;

        string host = "127.0.0.1";
        int port = 8000;

        try
        {
            _socket = new TcpClient(host, port);
            _stream = _socket.GetStream();
            _writter = new StreamWriter(_stream);
            _reader = new StreamReader(_stream);

            _socketReady = true;
            Debug.Log("----> client connect");
        }
        catch (Exception e)
        {
            Debug.Log("----> error: " + e);
        }
    }

    private void Send(string data)
    {
        if (!_socketReady)
            return;

        _writter.WriteLine(data);
        _writter.Flush();
    }

    public void OnSendButton()
    {
        string msg = GameObject.Find("SendInput").GetComponent<InputField>().text;
        Send(msg);
    }

    private void CloseSocket()
    {
        if (!_socketReady)
            return;

        _writter.Close();
        _reader.Close();
        _socket.Close();

        _socketReady = false;
    }

    private void OnApplicationQuit() => CloseSocket();
    private void OnDisable() => CloseSocket();
        
}