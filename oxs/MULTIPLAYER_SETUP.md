# Multiplayer Setup Guide

This document outlines the setup steps for each multiplayer mode in OXS.

## Current Status

| Mode | Status | Notes |
|------|--------|-------|
| **LAN** | ✅ Ready | Works with ENet, shows local IP |
| **WAN** | 🔧 Needs signaling server | WebRTC ready, needs server URL |
| **Steam** | 📋 Placeholder | Needs Steam SDK + App ID |

---

## LAN Multiplayer (Ready to Test)

LAN multiplayer is fully functional and ready to test.

### How to Test

1. Open the Godot project
2. Run two instances (or run on two computers on the same network)
3. **Instance 1 (Host)**:
   - Click **Multiplayer** → **LAN** tab
   - Click **Host Game**
   - Note the IP address displayed
4. **Instance 2 (Client)**:
   - Click **Multiplayer** → **LAN** tab
   - Enter the host's IP address
   - Click **Join Game**
5. Play a networked game!

### Features
- Automatic local IP detection
- Configurable port (default: 7777)
- Board size selection (3x3, 4x4, 5x5)
- Real-time move synchronization
- Disconnect detection

---

## WAN Multiplayer (Needs Signaling Server)

WAN multiplayer uses WebRTC for peer-to-peer connections over the internet. It requires a signaling server to establish the initial connection.

### Step 1: Deploy a Signaling Server

Options for signaling servers:

**Option A: Simple Node.js Server**
```bash
# Clone a simple signaling server
git clone https://github.com/nicholasbailey/simple-signaling-server
cd simple-signaling-server
npm install
npm start
```

**Option B: Self-Hosted WebSocket Relay**
- Any WebSocket server that can relay messages between peers
- Must support room-based message routing

**Option C: Cloud Deployment**
- Deploy to Render, Railway, Fly.io, or similar
- Ensure WebSocket support is enabled

### Step 2: Update Signaling URL

Edit `oxs/src/oxs/Networking/Signaling/SignalingClient.cs`:

```csharp
// Change this line:
private const string DefaultSignalingServer = "wss://your-signaling-server.example.com";
```

### Step 3: Test

1. Host creates a game → Gets a 6-character room code
2. Client enters the room code → Joins the game
3. WebRTC establishes peer-to-peer connection
4. Play!

### Signaling Protocol

The signaling server must handle these message types:
- `create_room` - Host creates a new room
- `join_room` - Client joins existing room
- `offer` / `answer` - WebRTC SDP exchange
- `ice_candidate` - ICE candidate exchange
- `peer_joined` / `peer_left` - Presence notifications

---

## Steam Multiplayer (Requires Setup)

Steam multiplayer requires a Steamworks account and SDK integration.

### Step 1: Register on Steamworks

1. Go to [Steamworks](https://partner.steamgames.com/)
2. Pay the $100 registration fee
3. Create your app and get an **App ID**

### Step 2: Add Dependencies

```bash
# Option A: Steamworks.NET
dotnet add package Steamworks.NET

# Option B: steam-multiplayer-peer-csharp (recommended for Godot)
# See: https://github.com/craethke/steam-multiplayer-peer-csharp
```

### Step 3: Create steam_appid.txt

Create a file `oxs/src/oxs/steam_appid.txt` containing only your App ID:
```
YOUR_APP_ID_HERE
```

### Step 4: Bundle Steam SDK DLLs

Download the Steamworks SDK and copy these files to your project:

**Windows:**
- `steam_api64.dll`

**Linux:**
- `libsteam_api.so`

**macOS:**
- `libsteam_api.dylib`

### Step 5: Implement Steam Integration

The placeholder code in `Networking/Transports/SteamPeerFactory.cs` needs to be completed:

1. Initialize Steam SDK on app startup
2. Implement lobby creation/joining
3. Implement friend invitations
4. Handle Steam callbacks

### Steam Features (When Implemented)
- Steam lobby system
- Friend invitations via Steam overlay
- Lobby browser
- Steam networking (NAT traversal handled by Steam)

---

## Testing Checklist

### LAN Testing
- [ ] Host game on Instance 1
- [ ] Join game on Instance 2 using IP
- [ ] Play full game, verify moves sync
- [ ] Test 3x3, 4x4, 5x5 board sizes
- [ ] Test win/draw detection
- [ ] Test rematch functionality
- [ ] Test disconnect handling (close one instance)
- [ ] Test reconnection scenarios

### WAN Testing
- [ ] Deploy signaling server
- [ ] Update SignalingClient URL
- [ ] Host creates room, gets room code
- [ ] Client joins using room code
- [ ] Verify WebRTC connection establishes
- [ ] Play full game through NAT
- [ ] Test with players on different networks

### Steam Testing
- [ ] Register Steamworks App ID
- [ ] Add Steam SDK dependencies
- [ ] Create steam_appid.txt
- [ ] Bundle Steam DLLs
- [ ] Implement SteamPeerFactory
- [ ] Test lobby creation
- [ ] Test friend invitations
- [ ] Test lobby browser

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                      MultiplayerMenu                         │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐                      │
│  │   LAN   │  │   WAN   │  │  Steam  │  ← Tab Selection     │
│  └────┬────┘  └────┬────┘  └────┬────┘                      │
└───────┼────────────┼────────────┼───────────────────────────┘
        │            │            │
        ▼            ▼            ▼
┌───────────────────────────────────────────────────────────────┐
│                  IMultiplayerPeerFactory                      │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐          │
│  │ENetPeerFactory│ │WebRtcPeer   │ │SteamPeer    │          │
│  │    (LAN)     │ │Factory (WAN)│ │Factory      │          │
│  └──────────────┘ └──────────────┘ └──────────────┘          │
└───────────────────────────────────────────────────────────────┘
        │            │            │
        ▼            ▼            ▼
┌───────────────────────────────────────────────────────────────┐
│                     NetworkManager                            │
│  - Manages MultiplayerPeer                                    │
│  - Handles connection events                                  │
│  - Routes RPC calls                                           │
└───────────────────────────────────────────────────────────────┘
        │
        ▼
┌───────────────────────────────────────────────────────────────┐
│                    GameSynchroniser                           │
│  - Synchronizes game state                                    │
│  - Validates moves (host authority)                           │
│  - Broadcasts confirmed moves                                 │
└───────────────────────────────────────────────────────────────┘
        │
        ▼
┌───────────────────────────────────────────────────────────────┐
│                 NetworkedGameController                       │
│  - Handles local player input                                 │
│  - Updates UI based on network events                         │
│  - Shows turn indicator and game results                      │
└───────────────────────────────────────────────────────────────┘
```

---

## Troubleshooting

### LAN: Can't find host
- Ensure both devices are on the same network
- Check firewall settings (allow port 7777)
- Verify the IP address is correct

### LAN: Connection timeout
- Check if the host is still running
- Try a different port
- Disable VPN if active

### WAN: Room code doesn't work
- Verify signaling server is running
- Check browser console for WebSocket errors
- Ensure room code is entered correctly (case-sensitive)

### WAN: Connection fails after room join
- NAT traversal may have failed
- Try using a TURN server for relay fallback
- Check if WebRTC is blocked by firewall

### Steam: "Steam not initialized"
- Ensure Steam client is running
- Verify steam_appid.txt exists and contains valid App ID
- Check that Steam SDK DLLs are in the correct location
