using Godot;
using OXS.Core;

namespace OXS.Networking;

public partial class GameSynchroniser : Node {
    [Signal]
    public delegate void LocalMoveRequestedEventHandler(int row, int col);

    [Signal]
    public delegate void RemoteMoveConfirmedEventHandler(int row, int col, int playerValue);

    [Signal]
    public delegate void PlayerAssignedEventHandler(int playerValue);

    [Signal]
    public delegate void GameStateSyncedEventHandler(byte[] boardData, int currentPlayer, int phase);

    private NetworkManager _networkManager = null!;
    private GameStateMachine _game = null!;
    private PlayerId _localPlayer;
    private long _remotePeerId;

    public PlayerId LocalPlayer => _localPlayer;
    public bool IsHost => _networkManager.IsHost;

    public override void _Ready() {
        _networkManager = GetNode<NetworkManager>("/root/NetworkManager");

        _networkManager.PlayerConnected += OnPlayerConnected;
        _networkManager.PlayerDisconnected += OnPlayerDisconnected;
        _networkManager.MoveReceived += OnMoveReceived;
        _networkManager.MoveConfirmedReceived += OnMoveConfirmedReceived;
        _networkManager.PlayerAssignmentReceived += OnPlayerAssignmentReceived;
        _networkManager.GameStateReceived += OnGameStateReceived;
    }

    public void Initialize(GameStateMachine game) {
        _game = game;
    }

    public void RequestMove(int row, int col) {
        if (_networkManager.IsHost) {
            // Host processes move directly
            ProcessMove(row, col, _localPlayer);
        } else {
            // Client sends move request to host
            _networkManager.Rpc(nameof(NetworkManager.SendMove), row, col);
        }
    }

    private void ProcessMove(int row, int col, PlayerId player) {
        if (_game.CurrentPlayer != player) {
            return;
        }

        var result = _game.MakeMove(row, col, player);
        if (result.IsSuccess) {
            // Broadcast confirmed move to all peers
            _networkManager.Rpc(nameof(NetworkManager.ConfirmMove), row, col, player.Value);
        }
    }

    private void OnPlayerConnected(long peerId) {
        if (!_networkManager.IsHost) {
            return;
        }

        _remotePeerId = peerId;

        // Host assigns players: host is X, client is O
        _localPlayer = PlayerId.X;
        _networkManager.RpcId(peerId, nameof(NetworkManager.AssignPlayer), PlayerId.O.Value);

        // Sync current game state
        SyncGameStateToClient(peerId);
    }

    private void OnPlayerDisconnected(long peerId) {
        // Handle disconnection - could pause game or return to menu
        GD.Print($"[GameSynchroniser] Player {peerId} disconnected");
    }

    private void OnMoveReceived(int row, int col, long fromPeer) {
        if (!_networkManager.IsHost) {
            return;
        }

        // Host validates and processes client's move
        ProcessMove(row, col, PlayerId.O);
    }

    private void OnMoveConfirmedReceived(int row, int col, int playerValue) {
        EmitSignal(SignalName.RemoteMoveConfirmed, row, col, playerValue);
    }

    private void OnPlayerAssignmentReceived(int playerValue) {
        _localPlayer = playerValue == 0 ? PlayerId.X : PlayerId.O;
        EmitSignal(SignalName.PlayerAssigned, playerValue);
    }

    private void OnGameStateReceived(byte[] boardData, int currentPlayer, int phase) {
        EmitSignal(SignalName.GameStateSynced, boardData, currentPlayer, phase);
    }

    private void SyncGameStateToClient(long peerId) {
        var boardData = SerializeBoard(_game.Board);
        _networkManager.RpcId(peerId, nameof(NetworkManager.SyncGameState),
            boardData, _game.CurrentPlayer.Value, (int)_game.Phase);
    }

    private static byte[] SerializeBoard(Board board) {
        var data = new byte[board.Cells.Length];
        for (int i = 0; i < board.Cells.Length; i++) {
            data[i] = (byte)board.Cells[i];
        }
        return data;
    }

    public static Board DeserializeBoard(byte[] data) {
        var size = (int)Math.Sqrt(data.Length);
        var board = new Board(size);

        for (int i = 0; i < data.Length; i++) {
            var state = (CellState)data[i];
            if (state != CellState.Empty) {
                var row = i / size;
                var col = i % size;
                var player = state == CellState.X ? PlayerId.X : PlayerId.O;
                board = board.WithMove(row, col, player);
            }
        }

        return board;
    }
}
