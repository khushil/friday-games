using Godot;

namespace OXS.Networking;

/// <summary>
/// Connection type for multiplayer games.
/// </summary>
public enum ConnectionType
{
    LAN,    // Local network via ENet
    WAN,    // Internet via WebRTC
    Steam   // Steam networking
}

/// <summary>
/// Configuration for establishing a connection.
/// </summary>
public record ConnectionConfig(
    string Address = "",
    int Port = 7777,
    ulong SteamId = 0,
    string RoomCode = ""
);

/// <summary>
/// Information about an available lobby/game to join.
/// </summary>
public record LobbyInfo(
    string Name,
    string Host,
    int PlayerCount,
    int MaxPlayers,
    ConnectionType Type,
    object Id  // ENet address, room code, or Steam lobby ID
);

/// <summary>
/// Factory interface for creating multiplayer peers.
/// Allows different transport implementations (ENet, WebRTC, Steam).
/// </summary>
public interface IMultiplayerPeerFactory
{
    /// <summary>
    /// The connection type this factory provides.
    /// </summary>
    ConnectionType Type { get; }

    /// <summary>
    /// Whether this transport is available (e.g., Steam requires Steam to be running).
    /// </summary>
    bool IsAvailable { get; }

    /// <summary>
    /// Creates a server/host peer.
    /// </summary>
    Error CreateServer(ConnectionConfig config, out MultiplayerPeer peer);

    /// <summary>
    /// Creates a client peer and connects to a server.
    /// </summary>
    Error CreateClient(ConnectionConfig config, out MultiplayerPeer peer);

    /// <summary>
    /// Gets a list of available lobbies/games to join.
    /// Returns empty list if discovery is not supported.
    /// </summary>
    System.Threading.Tasks.Task<System.Collections.Generic.List<LobbyInfo>> GetAvailableLobbiesAsync();

    /// <summary>
    /// Gets the local address to display to users (e.g., local IP for LAN).
    /// </summary>
    string GetDisplayAddress();
}
