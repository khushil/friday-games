using Godot;
using OXS.Core;

namespace OXS.Networking;

public partial class NetworkManager : Node {
	[Signal]
	public delegate void PlayerConnectedEventHandler(long peerId);

	[Signal]
	public delegate void PlayerDisconnectedEventHandler(long peerId);

	[Signal]
	public delegate void ConnectionFailedEventHandler();

	[Signal]
	public delegate void MoveReceivedEventHandler(int row, int col, long fromPeer);

	[Signal]
	public delegate void MoveConfirmedReceivedEventHandler(int row, int col, int playerValue);

	[Signal]
	public delegate void PlayerAssignmentReceivedEventHandler(int playerValue);

	[Signal]
	public delegate void GameStateReceivedEventHandler(byte[] boardData, int currentPlayer, int phase);

	private ENetMultiplayerPeer _peer = new();

	public bool IsHost { get; private set; }
	public new bool IsConnected => Multiplayer.HasMultiplayerPeer() && Multiplayer.GetUniqueId() != 0;

	public override void _Ready() {
		Multiplayer.PeerConnected += OnPeerConnected;
		Multiplayer.PeerDisconnected += OnPeerDisconnected;
		Multiplayer.ConnectedToServer += OnConnectedToServer;
		Multiplayer.ConnectionFailed += OnConnectionFailed;
		Multiplayer.ServerDisconnected += OnServerDisconnected;
	}

	public Error HostGame(int port = 7777) {
		_peer = new ENetMultiplayerPeer();
		var error = _peer.CreateServer(port);
		if (error == Error.Ok) {
			Multiplayer.MultiplayerPeer = _peer;
			IsHost = true;
			GD.Print($"[NetworkManager] Hosting on port {port}");
		}
		return error;
	}

	public Error JoinGame(string address, int port = 7777) {
		_peer = new ENetMultiplayerPeer();
		var error = _peer.CreateClient(address, port);
		if (error == Error.Ok) {
			Multiplayer.MultiplayerPeer = _peer;
			IsHost = false;
			GD.Print($"[NetworkManager] Connecting to {address}:{port}");
		}
		return error;
	}

	public void Disconnect() {
		if (Multiplayer.HasMultiplayerPeer()) {
			Multiplayer.MultiplayerPeer.Close();
			Multiplayer.MultiplayerPeer = null;
		}
	}

	// RPC Methods - called across network

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SendMove(int row, int col) {
		var senderId = Multiplayer.GetRemoteSenderId();
		EmitSignal(SignalName.MoveReceived, row, col, senderId);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void ConfirmMove(int row, int col, int playerValue) {
		EmitSignal(SignalName.MoveConfirmedReceived, row, col, playerValue);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void AssignPlayer(int playerValue) {
		EmitSignal(SignalName.PlayerAssignmentReceived, playerValue);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void SyncGameState(byte[] boardData, int currentPlayer, int phase) {
		EmitSignal(SignalName.GameStateReceived, boardData, currentPlayer, phase);
	}

	// Event Handlers

	private void OnPeerConnected(long id) {
		GD.Print($"[NetworkManager] Peer {id} connected");
		EmitSignal(SignalName.PlayerConnected, id);
	}

	private void OnPeerDisconnected(long id) {
		GD.Print($"[NetworkManager] Peer {id} disconnected");
		EmitSignal(SignalName.PlayerDisconnected, id);
	}

	private void OnConnectedToServer() {
		GD.Print("[NetworkManager] Connected to server");
	}

	private void OnConnectionFailed() {
		GD.Print("[NetworkManager] Connection failed");
		EmitSignal(SignalName.ConnectionFailed);
	}

	private void OnServerDisconnected() {
		GD.Print("[NetworkManager] Server disconnected");
		EmitSignal(SignalName.PlayerDisconnected, 1L);
	}
}
