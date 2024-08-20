using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Numerics;
using Goldmetal.UndeadSurvivor;


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

    private float mTransformX, mTransformY, mScaleX, mGunRotationZ;
    private int mPlayerHp;
    private bool bmIsShotOn;


    private void Start()
    {
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        client = new TcpClient(IPandPort.MyIpAddress, IPandPort.MyPortNum);
        stream = client.GetStream();

        byte[] playerIdBytes = new byte[4];
        stream.Read(playerIdBytes, 0, playerIdBytes.Length);
        playerId = BitConverter.ToInt32(playerIdBytes, 0);
        Debug.Log("연결 player ID: " + playerId);
    }

    private void Update()
    {
        SendMovementData();

        // Listen for data from server
        if (stream.DataAvailable)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string serverMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            if (serverMessage.StartsWith("Player"))
            {
                //플레이어가 접속하면 정보를 받고 다른플레이어 소환 하는 매서드 작성 예정
                Debug.Log(serverMessage); 
            }
            else
            {
              
                ReceiveMovementData(buffer);
            }
        }
    }

    private void SendMovementData()
    {
        byte[] message = new byte[28];
        Buffer.BlockCopy(BitConverter.GetBytes(playerId), 0, message, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(mTransformX), 0, message, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(mTransformY), 0, message, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(mScaleX), 0, message, 12, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(mGunRotationZ), 0, message, 16, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(mPlayerHp), 0, message, 20, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(bmIsShotOn), 0, message, 24, 4);

        stream.Write(message, 0, message.Length);
    }

    private void ReceiveMovementData(byte[] buffer)
    {
        int receivedPlayerId = BitConverter.ToInt32(buffer, 0);
        float receivedX = BitConverter.ToSingle(buffer, 4);
        float receivedY = BitConverter.ToSingle(buffer, 8);
        float receivedScale = BitConverter.ToSingle(buffer, 12);
        float gunRotationZ = BitConverter.ToSingle(buffer, 16);
        int playerHp = BitConverter.ToInt32(buffer, 20);
        bool mIsShotOn = BitConverter.ToBoolean(buffer, 24);


        UpdateOtherPlayerPosition(receivedPlayerId, receivedX, receivedY, receivedScale, gunRotationZ, playerHp, mIsShotOn);

    }

    private void UpdateOtherPlayerPosition(int playerId, float transformX, float transformY, float scaleX, float gunRotationZ, int playerHp, bool mIsShotOn)
    {
        //playerId를 통해 저장된 각 오브젝트들을 동기화 할 예정
    }

    private void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
    }

}
