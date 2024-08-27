using System.Net.Sockets;
using System.Net;
using System.Numerics;

class GameServer
{
    private TcpListener server;
    private ClientState[] clients = new ClientState[4];
    private int connectedClients = 0;

    const int PORTNUM = 5678;

    // 보간 설정 값
    private const float maxTimeElapsedThreshold = 2.0f;
    private const float maxPositionDelta = 0.1f; // 위치 변화의 최대 허용값

    public void Start()
    {
        server = new TcpListener(IPAddress.Any, PORTNUM);
        server.Start();
        Console.WriteLine($"서버 시작");

        while (true)
        {
            if (connectedClients < 4)
            {
                TcpClient tcpClient = server.AcceptTcpClient();
                NetworkStream stream = tcpClient.GetStream();

                int playerId = connectedClients;
                clients[playerId] = new ClientState
                {
                    TcpClient = tcpClient,
                    LastPredictedX = 0,
                    LastPredictedY = 0
                };

                byte[] playerIdMessage = new byte[8];
                int notifyCode = 99;
                Buffer.BlockCopy(BitConverter.GetBytes(notifyCode), 0, playerIdMessage, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(playerId), 0, playerIdMessage, 4, 4);
                stream.Write(playerIdMessage, 0, playerIdMessage.Length);

                connectedClients++;

                Console.WriteLine($"클라이언트 {playerId} 접속");

                Thread clientThread = new Thread(() => HandleClient(playerId));
                clientThread.Start();
            }
        }
    }

    private void HandleClient(int playerId)
    {
        ClientState client = clients[playerId];
        NetworkStream stream = client.TcpClient.GetStream();
        byte[] buffer = new byte[1024];

        NotifyOthersAboutNewPlayer(playerId);

        Thread rttThread = new Thread(() =>
        {
            while (true)
            {
                NetworkRTTAverage(client.TcpClient, playerId);
                Thread.Sleep(1000); // 1초 대기
            }
        });
        rttThread.Start();

        while (true)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                Console.WriteLine($"클라이언트 {playerId} 연결이 해제");
                clients[playerId] = null; // 연결 해제된 클라이언트 초기화
                connectedClients--;
                break;
            }

            int receivedPlayerId = BitConverter.ToInt32(buffer, 0);
            if (receivedPlayerId != playerId)
            {
                // playerId값과 receivedPlayerId가 다르면 잘못된 패킷
                continue;
            }
            float transformX = BitConverter.ToSingle(buffer, 4);
            float transformY = BitConverter.ToSingle(buffer, 8);
            float scaleX = BitConverter.ToSingle(buffer, 12);
            float gunRotationZ = BitConverter.ToSingle(buffer, 16);
            int playerHp = BitConverter.ToInt32(buffer, 20);
            bool mIsShotOn = buffer[24] == 1;

            float velocityX = BitConverter.ToSingle(buffer, 25);
            float velocityY = BitConverter.ToSingle(buffer, 29);
            long timestamp = BitConverter.ToInt64(buffer, 33);

            float predictedX, predictedY;
            CalculatePredictedPosition(playerId, transformX, transformY, velocityX, velocityY, timestamp, out predictedX, out predictedY);
            Console.WriteLine($"mPlayerId : {receivedPlayerId}, predictedX : {predictedX},predictedY : {predictedY},timestamp : {timestamp}");

            BroadcastMessage(receivedPlayerId, predictedX, predictedY, scaleX, gunRotationZ, playerHp, mIsShotOn, velocityX, velocityY);
        }

        client.TcpClient.Close();
        connectedClients--;
    }

    private void BroadcastMessage(int playerId, float predictedX, float predictedY, float scaleX, float gunRotationZ, int playerHp, bool mIsShotOn, float velocityX, float velocityY)
    {
        byte[] message = new byte[33];
        Buffer.BlockCopy(BitConverter.GetBytes(playerId), 0, message, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(predictedX), 0, message, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(predictedY), 0, message, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(scaleX), 0, message, 12, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(gunRotationZ), 0, message, 16, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(playerHp), 0, message, 20, 4);
        message[24] = mIsShotOn ? (byte)1 : (byte)0;
        Buffer.BlockCopy(BitConverter.GetBytes(velocityX), 0, message, 25, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(velocityY), 0, message, 29, 4);

        for (int i = 0; i < clients.Length; i++)
        {
            if (clients[i] != null && clients[i].TcpClient.Connected && i != playerId)
            {
                NetworkStream stream = clients[i].TcpClient.GetStream();
                stream.Write(message, 0, message.Length);
            }
        }
    }

    private void NotifyOthersAboutNewPlayer(int newPlayerId)
    {
        int notifyCode = 99;
        byte[] message = new byte[8];
        Console.WriteLine($"NotifyOthersAboutNewPlayer : newplayerId : {newPlayerId}, notifyCode : {notifyCode}");
        Buffer.BlockCopy(BitConverter.GetBytes(notifyCode), 0, message, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(newPlayerId), 0, message, 4, 4);

        for (int i = 0; i < clients.Length; i++)
        {
            if (clients[i] != null && clients[i].TcpClient.Connected && i != newPlayerId)
            {
                Console.WriteLine($"NotifyOthersAboutNewPlayer :  i : {i}");
                NetworkStream stream = clients[i].TcpClient.GetStream();
                stream.Write(message, 0, message.Length);
            }
        }
    }

    private void NetworkRTTAverage(TcpClient client, int playerId)
    {

        NetworkStream stream = client.GetStream();
        long serverTime = DateTime.UtcNow.Ticks;
        byte[] playerIdMessage = new byte[16];
        int notifyCode = 100;
        Buffer.BlockCopy(BitConverter.GetBytes(notifyCode), 0, playerIdMessage, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(playerId), 0, playerIdMessage, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(serverTime), 0, playerIdMessage, 8, 8);
        stream.Write(playerIdMessage, 0, playerIdMessage.Length);

    }

    private void CalculatePredictedPosition(int playerId, float currentX, float currentY, float velocityX, float velocityY, long timestamp, out float predictedX, out float predictedY)
    {
        long currentTime = DateTime.UtcNow.Ticks;
        float timeDiff = (currentTime - timestamp) / (float)TimeSpan.TicksPerSecond;
        Console.WriteLine($"시간 차이  :  timeDiff : {timeDiff}");

        Vector3 position = new Vector3(currentX, currentY, 0);
        Vector3 predictedPosition = position + new Vector3(velocityX * timeDiff, velocityY * timeDiff, 0);
        predictedX = predictedPosition.X;
        predictedY = predictedPosition.Y;
    }

    public static void Main(string[] args)
    {
        GameServer server = new GameServer();
        server.Start();
    }

    private class ClientState
    {
        public TcpClient TcpClient { get; set; }
        public float LastPredictedX { get; set; }
        public float LastPredictedY { get; set; }
        public float LastValidDeltaX { get; set; }
        public float LastValidDeltaY { get; set; }


    }
}
