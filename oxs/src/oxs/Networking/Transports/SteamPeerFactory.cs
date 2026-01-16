using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using OXS.Networking;

namespace OXS.Networking.Transports;

/// <summary>
/// Steam-based peer factory for Steam multiplayer.
/// Uses Steam networking for peer-to-peer connections with lobby support.
///
/// REQUIREMENTS TO ENABLE STEAM:
/// 1. Register on Steamworks ($100) and get an App ID
/// 2. Create steam_appid.txt in project root with your App ID
/// 3. Add steam-multiplayer-peer-csharp NuGet package
/// 4. Bundle Steam SDK DLLs (steam_api64.dll/libsteam_api.so)
/// 5. Initialize Steam SDK on app startup
///
/// See: https://github.com/craethke/steam-multiplayer-peer-csharp
/// </summary>
public class SteamPeerFactory : IMultiplayerPeerFactory
{
    private ulong _currentLobbyId;
    private bool _isSteamInitialized;

    public ConnectionType Type => ConnectionType.Steam;

    // Steam is only available if initialized properly
    public bool IsAvailable => _isSteamInitialized;

    public SteamPeerFactory()
    {
        // TODO: Initialize Steam SDK
        // _isSteamInitialized = SteamAPI.Init();
        _isSteamInitialized = false;

        if (!_isSteamInitialized)
        {
            GD.Print("[SteamPeerFactory] Steam not initialized. Steam features disabled.");
        }
    }

    public Error CreateServer(ConnectionConfig config, out MultiplayerPeer peer)
    {
        peer = null!;

        if (!_isSteamInitialized)
        {
            GD.PrintErr("[SteamPeerFactory] Steam not initialized. Cannot create server.");
            return Error.Failed;
        }

        // TODO: Implement Steam lobby creation
        // Steps:
        // 1. Create Steam lobby with SteamMatchmaking.CreateLobby
        // 2. Set lobby data (game name, settings)
        // 3. Create SteamMultiplayerPeer as server
        // 4. Wait for players to join

        /*
        var steamPeer = new SteamMultiplayerPeer();
        steamPeer.CreateServer(0); // Virtual port
        peer = steamPeer;

        // Create lobby
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 2);

        return Error.Ok;
        */

        GD.PrintErr("[SteamPeerFactory] Steam hosting not yet implemented.");
        return Error.Failed;
    }

    public Error CreateClient(ConnectionConfig config, out MultiplayerPeer peer)
    {
        peer = null!;

        if (!_isSteamInitialized)
        {
            GD.PrintErr("[SteamPeerFactory] Steam not initialized. Cannot join.");
            return Error.Failed;
        }

        // TODO: Implement Steam lobby joining
        // Steps:
        // 1. Get host's Steam ID from lobby data
        // 2. Create SteamMultiplayerPeer as client
        // 3. Connect to host

        /*
        var steamPeer = new SteamMultiplayerPeer();
        steamPeer.CreateClient(config.SteamId, 0); // Virtual port
        peer = steamPeer;

        return Error.Ok;
        */

        GD.PrintErr("[SteamPeerFactory] Steam joining not yet implemented.");
        return Error.Failed;
    }

    public async Task<List<LobbyInfo>> GetAvailableLobbiesAsync()
    {
        var lobbies = new List<LobbyInfo>();

        if (!_isSteamInitialized)
        {
            return lobbies;
        }

        // TODO: Query Steam lobbies
        // Steps:
        // 1. Use SteamMatchmaking.AddRequestLobbyListFilter* for filters
        // 2. Call SteamMatchmaking.RequestLobbyList()
        // 3. Wait for LobbyMatchList_t callback
        // 4. Iterate lobbies and create LobbyInfo objects

        /*
        SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
        var call = SteamMatchmaking.RequestLobbyList();
        // Wait for callback and populate lobbies list
        */

        await Task.CompletedTask;
        return lobbies;
    }

    public string GetDisplayAddress()
    {
        if (!_isSteamInitialized)
        {
            return "Steam not available";
        }

        // Return Steam username or lobby ID
        // return SteamFriends.GetPersonaName();
        return _currentLobbyId > 0 ? $"Lobby: {_currentLobbyId}" : "Not in lobby";
    }

    /// <summary>
    /// Join a specific Steam lobby by ID.
    /// </summary>
    public Error JoinLobby(ulong lobbyId, out MultiplayerPeer peer)
    {
        peer = null!;

        if (!_isSteamInitialized)
        {
            return Error.Failed;
        }

        // TODO: Implement lobby joining
        // SteamMatchmaking.JoinLobby(new CSteamID(lobbyId));

        GD.PrintErr("[SteamPeerFactory] Lobby joining not yet implemented.");
        return Error.Failed;
    }

    /// <summary>
    /// Invite a Steam friend to the current lobby.
    /// </summary>
    public bool InviteFriend(ulong friendId)
    {
        if (!_isSteamInitialized || _currentLobbyId == 0)
        {
            return false;
        }

        // TODO: Implement friend invitation
        // return SteamMatchmaking.InviteUserToLobby(new CSteamID(_currentLobbyId), new CSteamID(friendId));

        GD.PrintErr("[SteamPeerFactory] Friend invitation not yet implemented.");
        return false;
    }

    /// <summary>
    /// Leave the current Steam lobby.
    /// </summary>
    public void LeaveLobby()
    {
        if (_currentLobbyId > 0)
        {
            // TODO: Leave lobby
            // SteamMatchmaking.LeaveLobby(new CSteamID(_currentLobbyId));
            _currentLobbyId = 0;
        }
    }

    public void Shutdown()
    {
        LeaveLobby();

        if (_isSteamInitialized)
        {
            // TODO: Shutdown Steam
            // SteamAPI.Shutdown();
            _isSteamInitialized = false;
        }
    }
}
