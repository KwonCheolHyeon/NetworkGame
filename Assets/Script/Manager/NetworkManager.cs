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

    private bool bmIsStart;

    private float mTransformX, mTransformY, mScaleX, mGunRotationZ;
    private int mPlayerHp;
    private bool bmIsShotOn;

   

    private void Start()
    {
        bmIsStart = false;
    }

    public void ConnetStart() 
    {
        if(!bmIsStart) 
        {
            bmIsStart = true;
            ConnectToServer();
        }
    }

    private void ConnectToServer()
    {
        client = new TcpClient(IPandPort.MyIpAddress, IPandPort.MyPortNum);
        stream = client.GetStream();

        byte[] playerIdBytes = new byte[8];
        stream.Read(playerIdBytes, 0, playerIdBytes.Length);

        int discriminationCode = BitConverter.ToInt32(playerIdBytes, 0);
        playerId = BitConverter.ToInt32(playerIdBytes, 4);
        GameManager.Instance.PlayerSetting(playerId);
        Debug.Log("연결 player ID: " + playerId);
    }

    private void Update()
    {

        if (stream != null && stream.DataAvailable)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);


            if (bytesRead == 0) return;
   
            int discriminationCode = BitConverter.ToInt32(buffer, 0);
            int receivedPlayerId = BitConverter.ToInt32(buffer, 4);


            if (discriminationCode == 99) // 99를 받으면 접속 했다는 코드
            {
                Debug.Log($"{receivedPlayerId} 플레이어 접속");
                GameManager.Instance.PlayerSetting(receivedPlayerId);
            }
            else
            {
                ReceiveMovementData(buffer);
                Debug.Log($"연결확인 3{discriminationCode} discriminationCode");
            }
           
        }
    }

    public void SendMovementData(float tranformX, float transformY, float ScaleX, float GunRotationZ,int playerHp, bool shot)
    {
        mTransformX = tranformX;
        mTransformY = transformY;
        mScaleX = ScaleX;
        mGunRotationZ = GunRotationZ;
        mPlayerHp = playerHp;
        bmIsShotOn = shot;

        byte[] message = new byte[28];
        Buffer.BlockCopy(BitConverter.GetBytes(playerId), 0, message, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(mTransformX), 0, message, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(mTransformY), 0, message, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(mScaleX), 0, message, 12, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(mGunRotationZ), 0, message, 16, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(mPlayerHp), 0, message, 20, 4);
        message[24] = bmIsShotOn ? (byte)1 : (byte)0;

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

        Debug.Log($"receivedPlayerId : {receivedPlayerId}, receivedX : {receivedX}, receivedY : {receivedY}, receivedScale : {receivedScale}, gunRotationZ : {gunRotationZ}, mIsShotOn : {mIsShotOn}");

        GameManager.Instance.PlayerSYNC(receivedPlayerId, receivedX, receivedY, receivedScale, gunRotationZ, playerHp, mIsShotOn);
    }

    private void OnApplicationQuit()
    {
        stream.Close();
        client.Close();
    }

}
