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
                // 씬에서 NetworkManager 오브젝트를 찾아서 할당
                instance = FindObjectOfType<NetworkManager>();

                if (instance == null)
                {
                    Debug.LogError("씬에 NetworkManager 오브젝트가 존재하지 않습니다.");
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
    private long bTimeOffset = 0;
    private long bCumulativeAverageServerTime = 0;
    private int bNumberOfSamples = 0;
    private bool bmIsConnected;

    private void Awake()
    {
        // 싱글톤 중복 방지: 동일한 타입의 인스턴스가 이미 존재하면 현재 오브젝트를 파괴
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
            client = new TcpClient(IPandPort.MyIpAddress, IPandPort.MyPortNum);//내 IP주소, 포트번호
            stream = client.GetStream();

            ReceivePlayerId();

            if (mPlayerId < 5)
            {
                Debug.Log($"{mPlayerId} ConnectToServer() PlayerSetting playerId값");
                GameManager.Instance.PlayerSetting(mPlayerId);
            }
            else
            {
                Debug.LogError($"{mPlayerId} ConnectToServer() playerId값 오류");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"서버 연결 실패: {e.Message}");
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
                Debug.LogError("ConnectToServer 잘못된 데이터");
                return;
            }
            totalBytesRead += bytesRead;
        }

        int discriminationCode = BitConverter.ToInt32(playerIdBytes, 0);
        mPlayerId = BitConverter.ToInt32(playerIdBytes, 4);

        if (discriminationCode != 99)
        {
            Debug.LogError($"discriminationCode : {discriminationCode}  playerId : {mPlayerId} ConnectToServer() discriminationCode값이 다르다");
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
        long time = BitConverter.ToInt64(buffer, 8);
        if (discriminationCode == 99)
        {
            Debug.Log($"{receivedPlayerId} 플레이어 접속");
            GameManager.Instance.PlayerSetting(receivedPlayerId);
        }
        else if (discriminationCode == 100) 
        {
            UpdateAverageServerTime(time);
        }
        else if (discriminationCode >= 0 && discriminationCode < 4)
        {
            ProcessMovementData(buffer);
            Debug.Log($"연결확인 : {discriminationCode}  == discriminationCode");
        }
        else
        {
            Debug.Log($"잘못된값 : {discriminationCode}  == discriminationCode");
        }
    }

    public void SendMovementData(float transformX, float transformY, float scaleX, float gunRotationZ, int playerHp, bool isShot,float velocityX,float velocityY)
    {
        byte[] message = new byte[41];
        Buffer.BlockCopy(BitConverter.GetBytes(mPlayerId), 0, message, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(transformX), 0, message, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(transformY), 0, message, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(scaleX), 0, message, 12, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(gunRotationZ), 0, message, 16, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(playerHp), 0, message, 20, 4);
        message[24] = isShot ? (byte)1 : (byte)0;
        Buffer.BlockCopy(BitConverter.GetBytes(velocityX), 0, message, 25, 4); // X 축 속도 추가
        Buffer.BlockCopy(BitConverter.GetBytes(velocityY), 0, message, 29, 4); // Y 축 속도 추가
        long timestamp = DateTime.UtcNow.Ticks + bTimeOffset;
        Buffer.BlockCopy(BitConverter.GetBytes(timestamp), 0, message, 33, 8); // 타임스탬프 추가
        Debug.Log($"SendMovementData  mPlayerId : {mPlayerId}, transformX : {transformX},transformY : {transformY},timestamp : {timestamp}");
        if (timestamp == 0) 
        {
            Debug.LogWarning("timestamp 가 0");
        }
        stream.Write(message, 0, message.Length);
    }

    private void ProcessMovementData(byte[] buffer)
    {
        int receivedPlayerId = BitConverter.ToInt32(buffer, 0);
        float predictX = BitConverter.ToSingle(buffer, 4);
        float predictY = BitConverter.ToSingle(buffer, 8);
        float receivedScale = BitConverter.ToSingle(buffer, 12);
        float receivedGunRotationZ = BitConverter.ToSingle(buffer, 16);
        int receivedPlayerHp = BitConverter.ToInt32(buffer, 20);
        bool isShotOn = BitConverter.ToBoolean(buffer, 24);
        float velocityX = BitConverter.ToSingle(buffer, 25);
        float velocityY = BitConverter.ToSingle(buffer, 29);

        GameManager.Instance.PlayerSYNC(receivedPlayerId, predictX, predictY, receivedScale, receivedGunRotationZ, receivedPlayerHp, isShotOn, velocityX, velocityY);
    }

    private void UpdateAverageServerTime(long newServerTime)
    {
        
        long localTime = DateTime.UtcNow.Ticks;

        // 총 서버 시간의 합을 누적
        bCumulativeAverageServerTime += newServerTime - localTime;
    
        // 샘플 개수를 증가시킴
        bNumberOfSamples++;

        // 현재까지의 서버 시간 평균 계산
        long averageServerTime = bCumulativeAverageServerTime / bNumberOfSamples;
        // 오프셋 계산 (평균 서버 시간과 현재 로컬 시간의 차이)
        bTimeOffset = averageServerTime;

        
    }

    private void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }

}
