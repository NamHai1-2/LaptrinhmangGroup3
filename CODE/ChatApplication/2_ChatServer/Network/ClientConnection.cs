using System;
using System.IO;
using System.Net.Sockets;
using _1_SharedLibrary.Models;
using _1_SharedLibrary.Utils;

namespace _2_ChatServer.Network
{
    public class ClientConnection
    {
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _networkStream;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public string Username { get; set; }
        public string ClientEndPoint { get; private set; }

        public bool IsConnected
        {
            get { try { return _tcpClient != null && _tcpClient.Connected; } catch { return false; } }
        }

        public ClientConnection(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _networkStream = _tcpClient.GetStream();
            _reader = new StreamReader(_networkStream);
            _writer = new StreamWriter(_networkStream) { AutoFlush = true };
            ClientEndPoint = _tcpClient.Client.RemoteEndPoint?.ToString();
        }

        public MessagePacket ReceiveMessage()
        {
            try
            {
                string rawData = _reader.ReadLine();
                if (string.IsNullOrWhiteSpace(rawData)) return null;
                return JsonParser.Deserialize<MessagePacket>(rawData);
            }
            catch { return null; }
        }

        public bool SendMessage(MessagePacket packet)
        {
            try
            {
                if (!IsConnected || packet == null) return false;
                string json = JsonParser.Serialize(packet);
                _writer.WriteLine(json);
                return true;
            }
            catch { return false; }
        }

        public void Close()
        {
            try { _reader?.Close(); _writer?.Close(); _networkStream?.Close(); _tcpClient?.Close(); } catch { }
        }
    }
}