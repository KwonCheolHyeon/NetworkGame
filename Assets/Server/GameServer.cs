using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;


class GameServer
{
    private TcpListener server;
    private TcpClient[] clients = new TcpClient[4];
    private int connectedClients = 0;

    const int PORTNUM = 5678;

    public void Start()
    {
        server = new TcpListener(IPAddress.Any, PORTNUM);
        server.Start();

        while (true)
        {
            if (connectedClients < 4)
            {
                TcpClient client = server.AcceptTcpClient();
                int playerId = connectedClients;

                NetworkStream stream = client.GetStream();
                byte[] playerIdMessage = new byte[8];
                int notifyCode = 99;
                Buffer.BlockCopy(BitConverter.GetBytes(notifyCode), 0, playerIdMessage, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(playerId), 0, playerIdMessage, 4, 4);
                stream.Write(playerIdMessage, 0, playerIdMessage.Length);

                clients[connectedClients] = client;
                connectedClients++;

                Console.WriteLine($"클라이언트 {playerId} 접속");

                Thread clientThread = new Thread(() => HandleClient(client, playerId));
                clientThread.Start(); 
            }
        }
    }

    private void HandleClient(TcpClient client, int playerId)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];


        NotifyOthersAboutNewPlayer(playerId);

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
            float transformX = BitConverter.ToSingle(buffer, 4);
            float transformY = BitConverter.ToSingle(buffer, 8);
            float scaleX = BitConverter.ToSingle(buffer, 12);
            float gunRotationZ = BitConverter.ToSingle(buffer, 16);
            int playerHp = BitConverter.ToInt32(buffer, 20);
            bool mIsShotOn = buffer[24] == 1;



            BroadcastMessage(receivedPlayerId, transformX, transformY, scaleX, gunRotationZ, playerHp, mIsShotOn);
        }

        client.Close();
        connectedClients--;
    }

    private void BroadcastMessage(int playerId, float transformX, float transformY, float scaleX, float gunRotationZ, int playerHp, bool mIsShotOn)
    {
        byte[] message = new byte[25];
        Buffer.BlockCopy(BitConverter.GetBytes(playerId), 0, message, 0, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(transformX), 0, message, 4, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(transformY), 0, message, 8, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(scaleX), 0, message, 12, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(gunRotationZ), 0, message, 16, 4);
        Buffer.BlockCopy(BitConverter.GetBytes(playerHp), 0, message, 20, 4);

        // bool 값을 단일 바이트로 변환하여 직접 할당
        message[24] = mIsShotOn ? (byte)1 : (byte)0;

        for (int i = 0; i < clients.Length; i++)
        {
            if (clients[i] != null && clients[i].Connected && i != playerId)
            {
                NetworkStream stream = clients[i].GetStream();
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
            if (clients[i] != null && clients[i].Connected && i != newPlayerId)
            {
                Console.WriteLine($"NotifyOthersAboutNewPlayer :  i : {i}");
                NetworkStream stream = clients[i].GetStream();
                stream.Write(message, 0, message.Length);
            }
        }
    }

    public static void Main(string[] args)
    {
        GameServer server = new GameServer();
        server.Start();
    }
}
