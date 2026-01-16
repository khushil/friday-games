using Godot;
using OXS.Core;
using OXS.Core.AI;

namespace OXS.Presentation;

public partial class GameController : Control {
    [Export]
    public NodePath BoardViewPath { get; set; } = null!;

    [Export]
    public NodePath StatusLabelPath { get; set; } = null!;

    [Export]
    public NodePath RematchButtonPath { get; set; } = null!;

    private BoardView _boardView = null!;
    private Label _statusLabel = null!;
    private Button _rematchButton = null!;

    private GameStateMachine _game = null!;
    private GameConfig _config = null!;
    private IAIPlayer? _aiPlayer;
    private PlayerId _humanPlayer = PlayerId.X;

    public override void _Ready() {
        _boardView = GetNode<BoardView>(BoardViewPath);
        _statusLabel = GetNode<Label>(StatusLabelPath);
        _rematchButton = GetNode<Button>(RematchButtonPath);

        _boardView.CellClicked += OnCellClicked;
        _rematchButton.Pressed += OnRematchPressed;

        // Auto-start if config is pending from MainMenu
        var config = MainMenu.ConsumePendingConfig();
        if (config != null) {
            GD.Print($"[GameController] Starting with config: {config.Mode}, {config.BoardSize}x{config.BoardSize}");
            StartGame(config);
        }
    }

    public void StartGame(GameConfig config) {
        _config = config;
        _game = new GameStateMachine(config.BoardSize, config.WinLength);
        _game.GameEnded += OnGameEnded;

        if (config.Mode == GameMode.VsAI && config.AIDifficulty.HasValue) {
            _aiPlayer = AIFactory.Create(config.AIDifficulty.Value);
        }

        _boardView.Initialize(config.BoardSize);
        _rematchButton.Visible = false;
        UpdateStatus();
    }

    private void OnCellClicked(int row, int col) {
        if (_game == null || _game.Phase == GamePhase.GameOver) {
            return;
        }

        // In VsAI mode, only allow human player to move on their turn
        if (_config.Mode == GameMode.VsAI && _game.CurrentPlayer != _humanPlayer) {
            return;
        }

        var result = _game.MakeMove(row, col);
        if (result.IsSuccess) {
            _boardView.UpdateBoard(_game.Board);
            UpdateStatus();

            // If VsAI mode and game not over, trigger AI move
            if (_config.Mode == GameMode.VsAI &&
                _game.Phase == GamePhase.Playing &&
                _aiPlayer != null) {
                CallDeferred(nameof(MakeAIMove));
            }
        }
    }

    private void MakeAIMove() {
        if (_aiPlayer == null || _game.Phase == GamePhase.GameOver) {
            return;
        }

        var move = _aiPlayer.SelectMove(_game.Board, _game.CurrentPlayer, _config.WinLength);
        var result = _game.MakeMove(move.Row, move.Col);

        if (result.IsSuccess) {
            _boardView.UpdateBoard(_game.Board);
            UpdateStatus();
        }
    }

    private void OnGameEnded(GameResult result) {
        _rematchButton.Visible = true;

        var message = result switch {
            GameResult.Win win => $"{(win.Winner == PlayerId.X ? "X" : "O")} Wins!",
            GameResult.Draw => "It's a Draw!",
            _ => ""
        };

        _statusLabel.Text = message;
    }

    private void OnRematchPressed() {
        // Alternate starting player
        var startingPlayer = _game.CurrentPlayer == PlayerId.X ? PlayerId.O : PlayerId.X;
        _game.StartNextRound(startingPlayer);
        _boardView.Initialize(_config.BoardSize);
        _rematchButton.Visible = false;
        UpdateStatus();

        // If AI starts, trigger AI move
        if (_config.Mode == GameMode.VsAI &&
            _game.CurrentPlayer != _humanPlayer &&
            _aiPlayer != null) {
            CallDeferred(nameof(MakeAIMove));
        }
    }

    private void UpdateStatus() {
        if (_game != null && _game.Phase == GamePhase.Playing) {
            var player = _game.CurrentPlayer == PlayerId.X ? "X" : "O";
            _statusLabel.Text = $"{player}'s Turn";
        }
    }
}
