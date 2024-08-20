using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Numerics;
using System.Threading;


public class NetworkManager : MonoBehaviour
{
    private static NetworkManager instance;
    public static NetworkManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NetworkManager>();
                if (instance == null)
                {
                    GameObject singletonObject = new GameObject();
                    instance = singletonObject.AddComponent<NetworkManager>();

                    instance.Start();
                }
            }
            return instance;
        }
    }

    private TcpClient client;
    private NetworkStream stream;
    private int playerId;


    private void Start()
    {
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        client = new TcpClient("172.30.1.37", 6778);
        stream = client.GetStream();


        byte[] playerIdBytes = new byte[4];
        stream.Read(playerIdBytes, 0, playerIdBytes.Length);
        playerId = BitConverter.ToInt32(playerIdBytes, 0);
        Debug.Log("Connected to server with player ID: " + playerId);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessageToServer("Player " + playerId + " shot!");
        }
    }

    private void SendMessageToServer(string message)
    {
        byte[] messageBytes = Encoding.ASCII.GetBytes(message);
        stream.Write(messageBytes, 0, messageBytes.Length);
    }

    private void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
    }
}
