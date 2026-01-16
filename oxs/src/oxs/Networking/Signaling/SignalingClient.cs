using Godot;
using System;
using System.Text.Json;

namespace OXS.Networking.Signaling;

/// <summary>
/// WebSocket-based signaling client for WebRTC connection establishment.
/// Handles offer/answer/ICE candidate exchange for peer-to-peer connections.
/// </summary>
public partial class SignalingClient : Node
{
    // Default signaling server URL (can be configured)
    // For production, you would run your own signaling server or use a service
    private const string DefaultSignalingServer = "wss://your-signaling-server.example.com";

    private WebSocketPeer _webSocket = new();
    private string _signalingServerUrl;
    private string _roomCode = "";
    private int _localPeerId;
    private bool _isConnected;

    // Events for WebRTC signaling
    public event Action<int, string>? OnOfferReceived;        // peerId, sdp
    public event Action<int, string>? OnAnswerReceived;       // peerId, sdp
    public event Action<int, string, int, string>? OnIceCandidateReceived;  // peerId, media, index, name
    public event Action<int>? OnPeerJoined;                   // peerId
    public event Action<int>? OnPeerLeft;                     // peerId
    public event Action<string>? OnRoomCreated;               // roomCode
    public event Action? OnConnected;
    public event Action<string>? OnError;

    public SignalingClient(string? serverUrl = null)
    {
        _signalingServerUrl = serverUrl ?? DefaultSignalingServer;
        _localPeerId = new Random().Next(1000, 9999);
    }

    public override void _Process(double delta)
    {
        _webSocket.Poll();

        var state = _webSocket.GetReadyState();

        if (state == WebSocketPeer.State.Open)
        {
            while (_webSocket.GetAvailablePacketCount() > 0)
            {
                var packet = _webSocket.GetPacket();
                var message = System.Text.Encoding.UTF8.GetString(packet);
                ProcessMessage(message);
            }

            if (!_isConnected)
            {
                _isConnected = true;
                OnConnected?.Invoke();
            }
        }
        else if (state == WebSocketPeer.State.Closed)
        {
            if (_isConnected)
            {
                _isConnected = false;
                var code = _webSocket.GetCloseCode();
                var reason = _webSocket.GetCloseReason();
                GD.Print($"[SignalingClient] WebSocket closed: {code} - {reason}");
            }
        }
    }

    public void Connect()
    {
        var error = _webSocket.ConnectToUrl(_signalingServerUrl);
        if (error != Error.Ok)
        {
            GD.PrintErr($"[SignalingClient] Failed to connect to signaling server: {error}");
            OnError?.Invoke($"Failed to connect: {error}");
        }
        else
        {
            GD.Print($"[SignalingClient] Connecting to {_signalingServerUrl}");
        }
    }

    public void CreateRoom(string roomCode)
    {
        _roomCode = roomCode;
        Connect();

        // Send create room message after connection
        SendMessage(new SignalingMessage
        {
            Type = "create_room",
            RoomCode = roomCode,
            PeerId = _localPeerId
        });
    }

    public void JoinRoom(string roomCode)
    {
        _roomCode = roomCode;
        Connect();

        // Send join room message after connection
        SendMessage(new SignalingMessage
        {
            Type = "join_room",
            RoomCode = roomCode,
            PeerId = _localPeerId
        });
    }

    public void SendOffer(int targetPeerId, string sdp)
    {
        SendMessage(new SignalingMessage
        {
            Type = "offer",
            RoomCode = _roomCode,
            PeerId = _localPeerId,
            TargetPeerId = targetPeerId,
            Sdp = sdp
        });
    }

    public void SendAnswer(int targetPeerId, string sdp)
    {
        SendMessage(new SignalingMessage
        {
            Type = "answer",
            RoomCode = _roomCode,
            PeerId = _localPeerId,
            TargetPeerId = targetPeerId,
            Sdp = sdp
        });
    }

    public void SendIceCandidate(int targetPeerId, string media, int index, string name)
    {
        SendMessage(new SignalingMessage
        {
            Type = "ice_candidate",
            RoomCode = _roomCode,
            PeerId = _localPeerId,
            TargetPeerId = targetPeerId,
            IceCandidate = new IceCandidateData
            {
                Media = media,
                Index = index,
                Name = name
            }
        });
    }

    public void Disconnect()
    {
        if (_webSocket.GetReadyState() == WebSocketPeer.State.Open)
        {
            _webSocket.Close();
        }
    }

    private void SendMessage(SignalingMessage message)
    {
        if (_webSocket.GetReadyState() != WebSocketPeer.State.Open)
        {
            GD.PrintErr("[SignalingClient] Cannot send message: WebSocket not connected");
            return;
        }

        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var error = _webSocket.SendText(json);
        if (error != Error.Ok)
        {
            GD.PrintErr($"[SignalingClient] Failed to send message: {error}");
        }
    }

    private void ProcessMessage(string json)
    {
        try
        {
            var message = JsonSerializer.Deserialize<SignalingMessage>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (message == null)
            {
                GD.PrintErr("[SignalingClient] Received null message");
                return;
            }

            GD.Print($"[SignalingClient] Received message type: {message.Type}");

            switch (message.Type)
            {
                case "room_created":
                    OnRoomCreated?.Invoke(message.RoomCode ?? "");
                    break;

                case "peer_joined":
                    OnPeerJoined?.Invoke(message.PeerId);
                    break;

                case "peer_left":
                    OnPeerLeft?.Invoke(message.PeerId);
                    break;

                case "offer":
                    OnOfferReceived?.Invoke(message.PeerId, message.Sdp ?? "");
                    break;

                case "answer":
                    OnAnswerReceived?.Invoke(message.PeerId, message.Sdp ?? "");
                    break;

                case "ice_candidate":
                    if (message.IceCandidate != null)
                    {
                        OnIceCandidateReceived?.Invoke(
                            message.PeerId,
                            message.IceCandidate.Media,
                            message.IceCandidate.Index,
                            message.IceCandidate.Name
                        );
                    }
                    break;

                case "error":
                    OnError?.Invoke(message.Error ?? "Unknown error");
                    break;

                default:
                    GD.Print($"[SignalingClient] Unknown message type: {message.Type}");
                    break;
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"[SignalingClient] Failed to parse message: {e.Message}");
        }
    }
}

/// <summary>
/// Message format for signaling protocol.
/// </summary>
public class SignalingMessage
{
    public string Type { get; set; } = "";
    public string? RoomCode { get; set; }
    public int PeerId { get; set; }
    public int TargetPeerId { get; set; }
    public string? Sdp { get; set; }
    public IceCandidateData? IceCandidate { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// ICE candidate data for NAT traversal.
/// </summary>
public class IceCandidateData
{
    public string Media { get; set; } = "";
    public int Index { get; set; }
    public string Name { get; set; } = "";
}
