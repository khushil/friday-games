using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using OXS.Networking;
using OXS.Networking.Signaling;

namespace OXS.Networking.Transports;

/// <summary>
/// WebRTC-based peer factory for WAN multiplayer.
/// Enables peer-to-peer connections over the internet using WebRTC.
/// Requires a signaling server for connection establishment.
///
/// Note: WebRTC in Godot 4 requires careful setup. This implementation
/// provides the foundation but requires a signaling server to be deployed.
/// </summary>
public class WebRtcPeerFactory : IMultiplayerPeerFactory
{
    // Public STUN servers for NAT traversal
    private static readonly string[] StunServers =
    {
        "stun:stun.l.google.com:19302",
        "stun:stun1.l.google.com:19302"
    };

    private SignalingClient? _signalingClient;
    private WebRtcMultiplayerPeer? _peer;
    private Dictionary<int, WebRtcPeerConnection> _peerConnections = new();
    private string _roomCode = "";
    private bool _isHost;
    private int _uniqueId;

    public ConnectionType Type => ConnectionType.WAN;
    public bool IsAvailable => true; // WebRTC is built into Godot 4

    public string RoomCode => _roomCode;

    public Error CreateServer(ConnectionConfig config, out MultiplayerPeer peer)
    {
        _isHost = true;
        _uniqueId = 1; // Server is always 1
        _roomCode = GenerateRoomCode();

        _peer = new WebRtcMultiplayerPeer();
        var error = _peer.CreateServer();

        if (error == Error.Ok)
        {
            GD.Print($"[WebRtcPeerFactory] Created room: {_roomCode}");

            // Set up signaling
            SetupSignaling();
            _signalingClient?.CreateRoom(_roomCode);
        }
        else
        {
            GD.PrintErr($"[WebRtcPeerFactory] Failed to create server: {error}");
        }

        peer = _peer;
        return error;
    }

    public Error CreateClient(ConnectionConfig config, out MultiplayerPeer peer)
    {
        _isHost = false;
        _uniqueId = new System.Random().Next(2, 999999);
        _roomCode = config.RoomCode;

        _peer = new WebRtcMultiplayerPeer();
        var error = _peer.CreateClient(_uniqueId);

        if (error == Error.Ok)
        {
            GD.Print($"[WebRtcPeerFactory] Joining room: {_roomCode}");

            // Set up signaling
            SetupSignaling();
            _signalingClient?.JoinRoom(_roomCode);
        }
        else
        {
            GD.PrintErr($"[WebRtcPeerFactory] Failed to create client: {error}");
        }

        peer = _peer;
        return error;
    }

    public async Task<List<LobbyInfo>> GetAvailableLobbiesAsync()
    {
        // WebRTC doesn't support lobby discovery - rooms are joined via room code
        await Task.CompletedTask;
        return new List<LobbyInfo>();
    }

    public string GetDisplayAddress()
    {
        return _roomCode;
    }

    private void SetupSignaling()
    {
        _signalingClient = new SignalingClient();
        _signalingClient.OnOfferReceived += HandleOfferReceived;
        _signalingClient.OnAnswerReceived += HandleAnswerReceived;
        _signalingClient.OnIceCandidateReceived += HandleIceCandidateReceived;
        _signalingClient.OnPeerJoined += HandlePeerJoined;
    }

    private WebRtcPeerConnection CreatePeerConnection(int peerId)
    {
        var peerConnection = new WebRtcPeerConnection();

        // Create ICE servers config
        var iceServers = new Godot.Collections.Array();
        foreach (var server in StunServers)
        {
            var serverDict = new Godot.Collections.Dictionary
            {
                { "urls", server }
            };
            iceServers.Add(serverDict);
        }

        var config = new Godot.Collections.Dictionary
        {
            { "iceServers", iceServers }
        };

        var error = peerConnection.Initialize(config);
        if (error != Error.Ok)
        {
            GD.PrintErr($"[WebRtcPeerFactory] Failed to initialize peer connection: {error}");
            return peerConnection;
        }

        // Connect signals for offer/answer/ICE
        peerConnection.SessionDescriptionCreated += (type, sdp) =>
        {
            peerConnection.SetLocalDescription(type, sdp);

            if (type == "offer")
            {
                _signalingClient?.SendOffer(peerId, sdp);
            }
            else if (type == "answer")
            {
                _signalingClient?.SendAnswer(peerId, sdp);
            }
        };

        peerConnection.IceCandidateCreated += (media, index, name) =>
        {
            _signalingClient?.SendIceCandidate(peerId, media, (int)index, name);
        };

        _peerConnections[peerId] = peerConnection;

        // Add to multiplayer peer
        _peer?.AddPeer(peerConnection, peerId);

        return peerConnection;
    }

    private void HandleOfferReceived(int peerId, string sdp)
    {
        if (_peer == null) return;

        GD.Print($"[WebRtcPeerFactory] Received offer from peer {peerId}");

        // Create peer connection if doesn't exist
        if (!_peerConnections.TryGetValue(peerId, out var peerConnection))
        {
            peerConnection = CreatePeerConnection(peerId);
        }

        peerConnection.SetRemoteDescription("offer", sdp);
    }

    private void HandleAnswerReceived(int peerId, string sdp)
    {
        if (_peer == null) return;

        GD.Print($"[WebRtcPeerFactory] Received answer from peer {peerId}");

        if (_peerConnections.TryGetValue(peerId, out var peerConnection))
        {
            peerConnection.SetRemoteDescription("answer", sdp);
        }
    }

    private void HandleIceCandidateReceived(int peerId, string media, int index, string name)
    {
        if (_peer == null) return;

        if (_peerConnections.TryGetValue(peerId, out var peerConnection))
        {
            peerConnection.AddIceCandidate(media, index, name);
        }
    }

    private void HandlePeerJoined(int peerId)
    {
        if (_peer == null || !_isHost) return;

        GD.Print($"[WebRtcPeerFactory] Peer {peerId} joined, creating offer");

        // Host creates peer connection and offer when new peer joins
        var peerConnection = CreatePeerConnection(peerId);
        peerConnection.CreateOffer();
    }

    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // No I, O, 0, 1 to avoid confusion
        var random = new System.Random();
        var code = new char[6];
        for (int i = 0; i < 6; i++)
        {
            code[i] = chars[random.Next(chars.Length)];
        }
        return new string(code);
    }

    public void Disconnect()
    {
        _signalingClient?.Disconnect();
        _signalingClient = null;

        foreach (var peerConnection in _peerConnections.Values)
        {
            peerConnection.Close();
        }
        _peerConnections.Clear();

        _peer?.Close();
        _peer = null;
    }
}
