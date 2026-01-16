using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using OXS.Networking;

namespace OXS.Networking.Transports;

/// <summary>
/// ENet-based peer factory for local network (LAN) multiplayer.
/// </summary>
public class ENetPeerFactory : IMultiplayerPeerFactory
{
    public const int DefaultPort = 7777;
    public const int DiscoveryPort = 7776;

    public ConnectionType Type => ConnectionType.LAN;
    public bool IsAvailable => true;  // ENet is always available

    public Error CreateServer(ConnectionConfig config, out MultiplayerPeer peer)
    {
        var enetPeer = new ENetMultiplayerPeer();
        var port = config.Port > 0 ? config.Port : DefaultPort;
        var error = enetPeer.CreateServer(port);

        if (error == Error.Ok)
        {
            GD.Print($"[ENetPeerFactory] Server created on port {port}");
        }
        else
        {
            GD.PrintErr($"[ENetPeerFactory] Failed to create server: {error}");
        }

        peer = enetPeer;
        return error;
    }

    public Error CreateClient(ConnectionConfig config, out MultiplayerPeer peer)
    {
        var enetPeer = new ENetMultiplayerPeer();
        var port = config.Port > 0 ? config.Port : DefaultPort;
        var error = enetPeer.CreateClient(config.Address, port);

        if (error == Error.Ok)
        {
            GD.Print($"[ENetPeerFactory] Connecting to {config.Address}:{port}");
        }
        else
        {
            GD.PrintErr($"[ENetPeerFactory] Failed to connect: {error}");
        }

        peer = enetPeer;
        return error;
    }

    public async Task<List<LobbyInfo>> GetAvailableLobbiesAsync()
    {
        // For now, return empty list - discovery will be added later
        // LAN discovery requires UDP broadcast which is complex to implement
        await Task.CompletedTask;
        return new List<LobbyInfo>();
    }

    public string GetDisplayAddress()
    {
        return GetLocalIPAddress() ?? "Unknown";
    }

    /// <summary>
    /// Gets the local IP address for display to users.
    /// </summary>
    public static string? GetLocalIPAddress()
    {
        try
        {
            // Get all network interfaces
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up
                          && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            foreach (var ni in interfaces)
            {
                var props = ni.GetIPProperties();
                var ipv4 = props.UnicastAddresses
                    .FirstOrDefault(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork
                                         && !IPAddress.IsLoopback(addr.Address));

                if (ipv4 != null)
                {
                    return ipv4.Address.ToString();
                }
            }

            // Fallback: try to get any IPv4 address
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var fallback = host.AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            return fallback?.ToString();
        }
        catch
        {
            return null;
        }
    }
}
