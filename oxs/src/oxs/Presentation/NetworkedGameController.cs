using Godot;
using OXS.Core;
using OXS.Networking;
using OXS.Presentation.Theme;

namespace OXS.Presentation;

/// <summary>
/// Game controller for networked multiplayer games.
/// Uses GameSynchroniser to handle moves across the network.
/// </summary>
public partial class NetworkedGameController : Control
{
    private BoardView _boardView = null!;
    private Label _statusLabel = null!;
    private Label _playerLabel = null!;
    private Button _rematchButton = null!;
    private Button _backButton = null!;
    private ColorRect? _background;

    private GameStateMachine _game = null!;
    private GameSynchroniser _synchroniser = null!;
    private NetworkManager _networkManager = null!;

    private int _boardSize;
    private int _winLength;
    private PlayerId _localPlayer;
    private bool _isPlayerAssigned;
    private bool _gameStarted;

    public override void _Ready()
    {
        _networkManager = GetNode<NetworkManager>("/root/NetworkManager");

        SetupBackground();
        SetupUI();

        // Fade in animation
        Modulate = new Color(1, 1, 1, 0);
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1.0f, GameTheme.Animation.SceneTransition);

        // Get pending config from MultiplayerMenu
        var networkConfig = MultiplayerMenu.ConsumePendingNetworkConfig();
        _boardSize = MultiplayerMenu.ConsumePendingBoardSize();
        _winLength = _boardSize == 5 ? 4 : _boardSize;

        if (networkConfig == null)
        {
            GD.PrintErr("[NetworkedGameController] No pending network config!");
            ReturnToMenu();
            return;
        }

        GD.Print($"[NetworkedGameController] Starting networked game. IsHost: {networkConfig.IsHost}, BoardSize: {_boardSize}");

        // Create synchroniser
        _synchroniser = new GameSynchroniser();
        AddChild(_synchroniser);

        // Connect to synchroniser signals
        _synchroniser.RemoteMoveConfirmed += OnRemoteMoveConfirmed;
        _synchroniser.PlayerAssigned += OnPlayerAssigned;
        _synchroniser.GameStateSynced += OnGameStateSynced;

        // Connect to network manager for disconnection
        _networkManager.PlayerDisconnected += OnPlayerDisconnected;

        if (networkConfig.IsHost)
        {
            // Host starts the game immediately
            _localPlayer = PlayerId.X;
            _isPlayerAssigned = true;
            StartGame();
        }
        else
        {
            // Client waits for player assignment from host
            _statusLabel.Text = "Waiting for game to start...";
        }
    }

    public override void _ExitTree()
    {
        // Cleanup signal connections
        if (_networkManager != null)
        {
            _networkManager.PlayerDisconnected -= OnPlayerDisconnected;
        }
    }

    private void SetupBackground()
    {
        _background = GetNodeOrNull<ColorRect>("Background");
        if (_background == null)
        {
            _background = new ColorRect();
            _background.SetAnchorsPreset(LayoutPreset.FullRect);
            _background.Name = "Background";
            AddChild(_background);
            MoveChild(_background, 0);
        }

        var shader = GD.Load<Shader>("res://Presentation/Shaders/gradient_background.gdshader");
        if (shader != null)
        {
            var material = new ShaderMaterial { Shader = shader };
            material.SetShaderParameter("color_top", GameTheme.Colors.GradientStart);
            material.SetShaderParameter("color_bottom", GameTheme.Colors.GradientEnd);
            material.SetShaderParameter("noise_intensity", 0.02f);
            material.SetShaderParameter("time_scale", 0.1f);
            _background.Material = material;
        }
        else
        {
            _background.Color = GameTheme.Colors.GradientStart;
        }
    }

    private void SetupUI()
    {
        var vbox = GetNode<VBoxContainer>("VBoxContainer");

        // Player label (shows "You are X" or "You are O")
        _playerLabel = GetNode<Label>("VBoxContainer/PlayerLabel");
        StyleLabel(_playerLabel, 24);
        _playerLabel.Text = "";

        // Board view
        _boardView = GetNode<BoardView>("VBoxContainer/BoardView");
        _boardView.CellClicked += OnCellClicked;

        // Status label
        _statusLabel = GetNode<Label>("VBoxContainer/StatusLabel");
        StyleLabel(_statusLabel, 32);

        // Rematch button
        _rematchButton = GetNode<Button>("VBoxContainer/RematchButton");
        StyleButton(_rematchButton);
        _rematchButton.Pressed += OnRematchPressed;
        _rematchButton.Visible = false;

        // Back button
        _backButton = GetNode<Button>("VBoxContainer/BackButton");
        StyleButton(_backButton);
        _backButton.Pressed += OnBackPressed;
    }

    private void StyleLabel(Label label, int fontSize)
    {
        label.AddThemeColorOverride("font_color", GameTheme.Colors.TextPrimary);
        label.AddThemeFontSizeOverride("font_size", fontSize);
    }

    private void StyleButton(Button button)
    {
        button.AddThemeColorOverride("font_color", GameTheme.Colors.TextPrimary);
        button.AddThemeColorOverride("font_hover_color", GameTheme.Colors.TextPrimary);
        button.AddThemeColorOverride("font_pressed_color", GameTheme.Colors.TextSecondary);
        button.AddThemeFontSizeOverride("font_size", 20);

        var normalStyle = CreateGlassStyleBox(GameTheme.Colors.GlassBackground);
        var hoverStyle = CreateGlassStyleBox(GameTheme.Colors.ButtonHover);
        var pressedStyle = CreateGlassStyleBox(GameTheme.Colors.ButtonPressed);

        button.AddThemeStyleboxOverride("normal", normalStyle);
        button.AddThemeStyleboxOverride("hover", hoverStyle);
        button.AddThemeStyleboxOverride("pressed", pressedStyle);
        button.AddThemeStyleboxOverride("focus", normalStyle);

        button.MouseEntered += () => OnButtonHovered(button, true);
        button.MouseExited += () => OnButtonHovered(button, false);
    }

    private StyleBoxFlat CreateGlassStyleBox(Color bgColor)
    {
        var style = new StyleBoxFlat();
        style.BgColor = bgColor;
        style.BorderColor = GameTheme.Colors.GlassBorder;
        style.SetBorderWidthAll((int)GameTheme.Sizes.BorderWidth);
        style.SetCornerRadiusAll((int)GameTheme.Sizes.BorderRadius);
        style.ContentMarginLeft = 20;
        style.ContentMarginRight = 20;
        style.ContentMarginTop = 12;
        style.ContentMarginBottom = 12;
        return style;
    }

    private void OnButtonHovered(Button button, bool hovered)
    {
        var tween = CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Cubic);

        if (hovered)
        {
            tween.TweenProperty(button, "scale", new Vector2(1.02f, 1.02f), GameTheme.Animation.HoverEffect);
        }
        else
        {
            tween.TweenProperty(button, "scale", Vector2.One, GameTheme.Animation.HoverEffect);
        }
    }

    private void StartGame()
    {
        _game = new GameStateMachine(_boardSize, _winLength);
        _game.GameEnded += OnGameEnded;

        _synchroniser.Initialize(_game);

        _boardView.Initialize(_boardSize);
        _rematchButton.Visible = false;
        _gameStarted = true;

        UpdatePlayerLabel();
        UpdateStatus();

        GD.Print($"[NetworkedGameController] Game started. Local player: {_localPlayer}");
    }

    private void OnCellClicked(int row, int col)
    {
        if (!_gameStarted || _game == null || _game.Phase == GamePhase.GameOver)
        {
            return;
        }

        // Only allow moves on local player's turn
        if (_game.CurrentPlayer != _localPlayer)
        {
            GD.Print($"[NetworkedGameController] Not your turn. Current: {_game.CurrentPlayer}, Local: {_localPlayer}");
            return;
        }

        // Send move through synchroniser (handles host/client logic)
        _synchroniser.RequestMove(row, col);
    }

    private void OnRemoteMoveConfirmed(int row, int col, int playerValue)
    {
        if (_game == null)
        {
            return;
        }

        var player = playerValue == 0 ? PlayerId.X : PlayerId.O;

        GD.Print($"[NetworkedGameController] Move confirmed: ({row},{col}) by {player}");

        // Update local game state
        var result = _game.MakeMove(row, col, player);
        if (result.IsSuccess)
        {
            _boardView.UpdateBoard(_game.Board);
            UpdateStatus();
        }
    }

    private void OnPlayerAssigned(int playerValue)
    {
        _localPlayer = playerValue == 0 ? PlayerId.X : PlayerId.O;
        _isPlayerAssigned = true;

        GD.Print($"[NetworkedGameController] Player assigned: {_localPlayer}");

        // Client can now start the game
        StartGame();
    }

    private void OnGameStateSynced(byte[] boardData, int currentPlayer, int phase)
    {
        if (_game == null)
        {
            return;
        }

        GD.Print($"[NetworkedGameController] Game state synced");

        // Reconstruct board from data
        var board = GameSynchroniser.DeserializeBoard(boardData);
        // Note: We'd need to update the internal game state here
        // For now, just update the view
        _boardView.UpdateBoard(board);
        UpdateStatus();
    }

    private void OnPlayerDisconnected(long peerId)
    {
        GD.Print($"[NetworkedGameController] Opponent disconnected: {peerId}");

        _statusLabel.Text = "Opponent disconnected!";
        _rematchButton.Visible = false;

        // Show back button more prominently
        _backButton.Text = "Return to Menu";
    }

    private void OnGameEnded(GameResult result)
    {
        // Highlight winning cells if there's a winner
        if (result is GameResult.Win win)
        {
            var winningPositions = new List<(int Row, int Col)>();
            foreach (var move in win.WinningLine)
            {
                winningPositions.Add((move.Row, move.Col));
            }
            _boardView.HighlightWinningCells(winningPositions);
        }

        var message = result switch
        {
            GameResult.Win w => w.Winner == _localPlayer ? "You Win!" : "You Lose!",
            GameResult.Draw => "It's a Draw!",
            _ => ""
        };

        // Animate status text appearance
        _statusLabel.Modulate = new Color(1, 1, 1, 0);
        _statusLabel.Text = message;

        var textTween = CreateTween();
        textTween.SetEase(Tween.EaseType.Out);
        textTween.TweenProperty(_statusLabel, "modulate:a", 1.0f, 0.3f);

        // Only host shows rematch button (host controls game flow)
        if (_networkManager.IsHost)
        {
            _rematchButton.Visible = true;
            _rematchButton.Modulate = new Color(1, 1, 1, 0);
            _rematchButton.Scale = new Vector2(0.8f, 0.8f);

            var btnTween = CreateTween();
            btnTween.SetParallel(true);
            btnTween.SetEase(Tween.EaseType.Out);
            btnTween.SetTrans(Tween.TransitionType.Back);
            btnTween.TweenProperty(_rematchButton, "modulate:a", 1.0f, 0.3f).SetDelay(0.2f);
            btnTween.TweenProperty(_rematchButton, "scale", Vector2.One, 0.3f).SetDelay(0.2f);
        }
    }

    private void OnRematchPressed()
    {
        // Reset highlights
        _boardView.ResetHighlights();

        // Alternate starting player
        var startingPlayer = _game.CurrentPlayer == PlayerId.X ? PlayerId.O : PlayerId.X;
        _game.StartNextRound(startingPlayer);
        _boardView.Initialize(_boardSize);
        _rematchButton.Visible = false;
        UpdateStatus();

        // TODO: Sync rematch to client
    }

    private void OnBackPressed()
    {
        // Disconnect from network
        _networkManager.Disconnect();
        ReturnToMenu();
    }

    private void ReturnToMenu()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0.0f, GameTheme.Animation.SceneTransition);
        tween.TweenCallback(Callable.From(LoadMainMenu));
    }

    private void LoadMainMenu()
    {
        var scene = GD.Load<PackedScene>("res://Presentation/Scenes/Main.tscn");
        if (scene == null)
        {
            GD.PrintErr("[NetworkedGameController] Failed to load Main.tscn!");
            return;
        }
        GetTree().ChangeSceneToPacked(scene);
    }

    private void UpdatePlayerLabel()
    {
        var playerSymbol = _localPlayer == PlayerId.X ? "X" : "O";
        _playerLabel.Text = $"You are {playerSymbol}";
    }

    private void UpdateStatus()
    {
        if (_game != null && _game.Phase == GamePhase.Playing)
        {
            if (_game.CurrentPlayer == _localPlayer)
            {
                _statusLabel.Text = "Your Turn";
            }
            else
            {
                _statusLabel.Text = "Opponent's Turn";
            }
        }
    }
}
