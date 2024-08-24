using UnityEngine;
using System;
using System.Net.Sockets;

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

                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }

    private TcpClient client;
    private NetworkStream stream;
    private int mPlayerId;
    private bool bmIsConnected;
    private void Awake()
    {
        // �̱��� �ߺ� ����: ������ Ÿ���� �ν��Ͻ��� �̹� �����ϸ� ���� ������Ʈ�� �ı�
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        bmIsConnected = false;
    }
    public void ConnectStart()
    {
        if (!bmIsConnected)
        {
            ConnectToServer();
            bmIsConnected = true;
        }
    }

    private void ConnectToServer()
    {
        try
        {
            client = new TcpClient(IPandPort.MyIpAddress, IPandPort.MyPortNum);//�� IP�ּ�, ��Ʈ��ȣ
            stream = client.GetStream();

            ReceivePlayerId();

            if (mPlayerId < 5)
            {
                Debug.Log($"{mPlayerId} ConnectToServer() PlayerSetting playerId��");
                GameManager.Instance.PlayerSetting(mPlayerId);
            }
            else
            {
                Debug.LogError($"{mPlayerId} ConnectToServer() playerId�� ����");
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
        mPlayerId = BitConverter.ToInt32(playerIdBytes, 4);

        if (discriminationCode != 99)
        {
            Debug.LogError($"discriminationCode : {discriminationCode}  playerId : {mPlayerId} ConnectToServer() discriminationCode���� �ٸ���");
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

    public void SendMovementData(float transformX, float transformY, float scaleX, float gunRotationZ, int playerHp, bool isShot)
    {
        byte[] message = new byte[28];
        Buffer.BlockCopy(BitConverter.GetBytes(mPlayerId), 0, message, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(transformX), 0, message, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(transformY), 0, message, 8, 4);
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
