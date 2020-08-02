using Godot;
using System.Collections.Concurrent;
using Rose.Network.Packets;
using FlatBuffers;

public class NetworkManager : Node
{
    [Signal]
    public delegate void DisconnectedSignal();
    [Signal]
    public delegate void LoginReplySignal(LoginReplyValue reply);

    private StreamPeerTCP Socket = null;
    private ConcurrentQueue<byte[]> DataToSend;
    private ConcurrentQueue<PacketData> DataReceived;

    public string Host = "127.0.0.1";
    public int port = 12345;
    private volatile bool Connected = false;
    private System.Threading.Thread ThreadIO;
    private int BytesSent = 0;
    private byte[] BytesReceived = null;

    private void RunIO()
    {
        while (Connected)
        {
            SendIO();
            ReceiveIO();
            System.Threading.Thread.Sleep(10); // We sleep for 10ms
        }
    }

    private void SendIO()
    {
        if (!Connected)
            return;
        
        // now we send any waiting packet
        if (DataToSend.Count > 0)
        {
            GD.Print("Sending packet");
            if (DataToSend.TryDequeue(out byte[] buffer))
            {
                Socket.PutU16((ushort)buffer.Length);
                if (Socket.PutData(buffer) != Error.Ok)
                {
                    GD.PrintErr("Error while sending data");
                    Connected = false;
                    Socket = null;
                }
            }
        }
    }

    private void ReceiveIO()
    {
        if (!Connected)
            return;

        // now we receive any waiting data
        if (Socket.GetAvailableBytes() > 0)
        {
            int size = Socket.GetU16();
            var data = Socket.GetData(size);
            if ((Error)data[0] == Error.Ok)
            {
                ReceiveData(data[1] as byte[]);
            } else
            {
                GD.PrintErr("Error while receiving data from the network");
                Connected = false;
                Socket = null;
            }
        }
    }

    public override void _Ready()
    {
        DataToSend = new ConcurrentQueue<byte[]>();
        DataReceived = new ConcurrentQueue<PacketData>();
        Socket = new StreamPeerTCP();
        if (ConnectToHost(Host, port))
            Connected = true;
        ThreadIO = new System.Threading.Thread(new System.Threading.ThreadStart(this.RunIO));
        ThreadIO.IsBackground = true;
        ThreadIO.Start();
    }

    public bool ConnectToHost(string host, int port)
    {
        if (Socket.ConnectToHost(host, port) != Error.Ok)
        {
            GD.PrintErr("Error while trying to connect to " + host + ":" + port);
            return false;
        }
        GD.Print("Connected to " + host + ":" + port);
        return true;
    }

    public override void _Process(float delta)
    {
        if (Socket == null)
            return;

        switch (Socket.GetStatus())
        {
            case StreamPeerTCP.Status.None:
                if (Connected)
                {
                    Socket = null;
                    Connected = false;
                    GD.Print("Disconnected");
                    EmitSignal("DisconnectedSignal");
                }
                return;
            case StreamPeerTCP.Status.Error:
                Connected = false;
                Socket = null;
                GD.PrintErr("Error with the server connection");
                EmitSignal("DisconnectedSignal");
                return;
            case StreamPeerTCP.Status.Connected:
                break;
        }

        // for each packet we trigger signals
        while (DataReceived.Count > 0)
        {
            if (DataReceived.TryDequeue(out PacketData packet))
            {
                switch (packet.DataType)
                {
                    case PacketType.LoginReply:
                        var reply = (LoginReply)packet.Data<LoginReply>();
                        EmitSignal("LoginReplySignal", reply.Reply);
                        break;
                }
            }
        }
    }

    public override void _ExitTree() {
        Connected = false;
        ThreadIO.Join();
        Socket.Dispose();
    }

    // ---------------- PACKETS ---------------------------
    public void SendLoginRequest(string username, string password)
    {
        SendPacket(CreateLoginRequest(username, password));
    }

    private byte[] CreateLoginRequest(string username, string password)
    {
        var builder = new FlatBufferBuilder(1024);
        var usernameOffset = builder.CreateString(username);
        var passwordOffset = builder.CreateString(password);
        LoginRequest.StartLoginRequest(builder);
        LoginRequest.AddUsername(builder, usernameOffset);
        LoginRequest.AddPassword(builder, passwordOffset);
        var login = LoginRequest.EndLoginRequest(builder);
        PacketData.StartPacketData(builder);
        PacketData.AddDataType(builder, PacketType.LoginRequest);
        PacketData.AddData(builder, login.Value);
        var request = PacketData.EndPacketData(builder);
        builder.Finish(request.Value);
        return builder.SizedByteArray();
    }

    public PacketData ParseBuffer(byte[] bytes)
    {
        var buffer = new ByteBuffer(bytes);
        return PacketData.GetRootAsPacketData(buffer);
    }

    private void SendPacket(byte[] buffer)
    {
        DataToSend.Enqueue(buffer);
    }

    private void ReceiveData(byte[] buffer)
    {
        DataReceived.Enqueue(ParseBuffer(buffer));
    }
}
