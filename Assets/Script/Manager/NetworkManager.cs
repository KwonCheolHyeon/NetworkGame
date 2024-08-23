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
                // ������ NetworkManager ������Ʈ�� ã�Ƽ� �Ҵ�
                instance = FindObjectOfType<NetworkManager>();

                if (instance == null)
                {
                    Debug.LogError("���� NetworkManager ������Ʈ�� �������� �ʽ��ϴ�.");
                    return null;
                }

                instance.Initialize();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }

    private TcpClient client;
    private NetworkStream stream;
    private int playerId;
    private bool isConnected;
    private void Awake()
    {
        // �̱��� �ߺ� ����: ������ Ÿ���� �ν��Ͻ��� �̹� �����ϸ� ���� ������Ʈ�� �ı�
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

    }
    private void Initialize()
    {
        isConnected = false;
    }
    public void ConnectStart()
    {
        if (!isConnected)
        {
            ConnectToServer();
            isConnected = true;
        }
    }

    private void ConnectToServer()
    {
        try
        {
            client = new TcpClient(IPandPort.MyIpAddress, IPandPort.MyPortNum);
            stream = client.GetStream();

            ReceivePlayerId();

            if (playerId < 5)
            {
                Debug.Log($"{playerId} ConnectToServer() PlayerSetting playerId��");
                GameManager.Instance.PlayerSetting(playerId);
            }
            else
            {
                Debug.LogError($"{playerId} ConnectToServer() playerId�� ����");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"���� ���� ����: {e.Message}");
        }
    }

    private void ReceivePlayerId()
    {
        byte[] playerIdBytes = new byte[8];
        int totalBytesRead = 0;

        while (totalBytesRead < playerIdBytes.Length)
        {
            int bytesRead = stream.Read(playerIdBytes, totalBytesRead, playerIdBytes.Length - totalBytesRead);
            if (bytesRead == 0)
            {
                Debug.LogError("ConnectToServer �߸��� ������");
                return;
            }
            totalBytesRead += bytesRead;
        }

        int discriminationCode = BitConverter.ToInt32(playerIdBytes, 0);
        playerId = BitConverter.ToInt32(playerIdBytes, 4);

        if (discriminationCode != 99)
        {
            Debug.LogError($"discriminationCode : {discriminationCode}  playerId : {playerId} ConnectToServer() discriminationCode���� �ٸ���");
        }
    }


    private void Update()
    {

        if (stream != null && stream.DataAvailable)
        {
            HandleIncomingData();
        }
    }

    private void HandleIncomingData()
    {
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        if (bytesRead == 0) return;

        int discriminationCode = BitConverter.ToInt32(buffer, 0);
        int receivedPlayerId = BitConverter.ToInt32(buffer, 4);

        if (discriminationCode == 99)
        {
            Debug.Log($"{receivedPlayerId} �÷��̾� ����");
            GameManager.Instance.PlayerSetting(receivedPlayerId);
        }
        else
        {
            ProcessMovementData(buffer);
            Debug.Log($"����Ȯ�� 3{discriminationCode} discriminationCode");
        }
    }

    public void SendMovementData(float x, float y, float scaleX, float gunRotationZ, int playerHp, bool isShot)
    {
        byte[] message = new byte[28];
        Buffer.BlockCopy(BitConverter.GetBytes(playerId), 0, message, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(x), 0, message, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(y), 0, message, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(scaleX), 0, message, 12, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(gunRotationZ), 0, message, 16, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(playerHp), 0, message, 20, 4);
        message[24] = isShot ? (byte)1 : (byte)0;

        stream.Write(message, 0, message.Length);
    }

    private void ProcessMovementData(byte[] buffer)
    {
        int receivedPlayerId = BitConverter.ToInt32(buffer, 0);
        float receivedX = BitConverter.ToSingle(buffer, 4);
        float receivedY = BitConverter.ToSingle(buffer, 8);
        float receivedScale = BitConverter.ToSingle(buffer, 12);
        float receivedGunRotationZ = BitConverter.ToSingle(buffer, 16);
        int receivedPlayerHp = BitConverter.ToInt32(buffer, 20);
        bool isShotOn = BitConverter.ToBoolean(buffer, 24);

        Debug.Log($"Player ID: {receivedPlayerId}, X: {receivedX}, Y: {receivedY}, Scale: {receivedScale}, Gun Rotation: {receivedGunRotationZ}, Is Shot: {isShotOn}");

        GameManager.Instance.PlayerSYNC(receivedPlayerId, receivedX, receivedY, receivedScale, receivedGunRotationZ, receivedPlayerHp, isShotOn);
    }

    private void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }

}
