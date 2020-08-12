using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    private List<ServerClient> _clients;
    private List<ServerClient> _disconnectList;

    public int port = 8000;
    private TcpListener _server;
    private bool _serverStarted;

    private void Start()
    {
        _clients = new List<ServerClient>();
        _disconnectList = new List<ServerClient>();

        try
        {
            _server = new TcpListener(IPAddress.Any, port);
            _server.Start();

            StartListening();

            _serverStarted = true;
            Debug.Log("----> server start");
        }
        catch (Exception e)
        {
            Debug.Log("----> Socket error: " + e);
        }
    }

    private void Update()
    {
        if (!_serverStarted)
            return;

        foreach (var c in _clients)
        {
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                _disconnectList.Add(c);
                continue;
            }

            else
            {
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();

                    if (data != null)
                    {
                        OnIncomingData(c, data);
                    }
                }
            }
        }
    }
    private void OnIncomingData(ServerClient c, string data)
    {
        Debug.Log($"{c.clientName}: + {data}");
        BroadCast(data, _clients);
    }

    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                else
                    return true;
            }
            else
                return false;

        }
        catch (Exception e)
        {
            Debug.Log($"----> exception: {e}");
            return false;
        }
    }

    private void StartListening()
    {
        _server.BeginAcceptTcpClient(AcceptTcpClient, _server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        _clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
        StartListening();

        BroadCast($"{_clients[_clients.Count - 1].clientName} has connected"
            , _clients);
    }

    private void BroadCast(string data, List<ServerClient> cl)
    {
        foreach (var c in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log($"----> write error: {e.Message} + to client: {c.clientName}");
            }
        }
    }
}

public class ServerClient
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket)
    {
        if (string.IsNullOrEmpty(clientName))
            clientName = "Guest";

        tcp = clientSocket;
    }

}
